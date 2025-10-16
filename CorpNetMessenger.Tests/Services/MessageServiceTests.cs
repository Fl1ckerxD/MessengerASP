using System.Linq.Expressions;
using AutoMapper;
using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Domain.MappingProfiles;
using CorpNetMessenger.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace CorpNetMessenger.Tests.Services
{
    public class MessageServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<MessageService>> _mockLogger;
        private readonly IMapper _mapper;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<IChatCacheService> _mockChatCacheService;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IChatService> _mockChatService;
        private readonly Mock<IMessageQueue<Message>> _mockMessageQueue;
        private readonly MessageService _messageService;

        public MessageServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<MessageService>>();
            _mockCache = new Mock<IMemoryCache>();
            _mockChatCacheService = new Mock<IChatCacheService>();
            _mockChatService = new Mock<IChatService>();
            _mockMessageQueue = new Mock<IMessageQueue<Message>>();

            var userStoreMock = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AppMappingProfile()); 
            });
            _mapper = config.CreateMapper();

            _messageService = new MessageService(
                _mockUnitOfWork.Object,
                _mockLogger.Object,
                _mapper,
                _mockUserManager.Object,
                _mockCache.Object,
                _mockChatCacheService.Object,
                _mockChatService.Object,
                _mockMessageQueue.Object);

        }

        [Fact]
        public async Task SaveMessage_WhenRequestIsNull_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _messageService.SaveMessageAsync(null, "user1"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task SaveMessage_WhenUserIdIsInvalid_ThrowsArgumentException(string? userId)
        {
            if (userId == null)
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _messageService.SaveMessageAsync(new ChatMessageDto(), userId));
                return;
            }
            await Assert.ThrowsAsync<ArgumentException>(() =>
            _messageService.SaveMessageAsync(new(), userId));
        }

        [Fact]
        public async Task SaveMessageAsync_ShouldEnqueueAndReturnMessageDtoWithUser()
        {
            var request = new ChatMessageDto
            {
                ChatId = "chat1",
                Text = "test message",
                Files = new List<Attachment>()
            };
            var userId = "user1";
            var expectedMessageId = "msg123";

            _mockChatService.Setup(c => c.UserInChatAsync(request.ChatId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMessageQueue.Setup(mq => mq.EnqueueAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
                
            var user = new User { Id = userId, Name = "John", LastName = "Doe"};
            _mockUserManager.Setup(u => u.FindByIdAsync(userId))
                .ReturnsAsync(user);

            var result = await _messageService.SaveMessageAsync(request, userId);

            result.Should().NotBeNull();
            result.Text.Should().Be(request.Text);
            result.Id.Should().NotBeNullOrEmpty();
            result.User.Should().NotBeNull();
            result.User.Id.Should().Be(userId);
            result.User.FullName.Should().Be($"{user.LastName} {user.Name}");
        }

        [Fact]
        public async Task SaveMessage_WhenUserNotInChat_ThrowsUnauthorizedAccessException()
        {
            var request = new ChatMessageDto { ChatId = "chat1" };
            var userId = "user1";

            _mockUnitOfWork.Setup(u => u.ChatUsers.GetByPredicateAsync(
                It.IsAny<Expression<Func<ChatUser, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ChatUser)null);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _messageService.SaveMessageAsync(request, userId));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task EditMessage_WhenNewTextIsInvalid_ReturnOperationResultFalse(string? newText)
        {
            string userId = "user1";

            _mockUnitOfWork.Setup(u => u.Messages.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Message { UserId = userId });

            var result = await _messageService.EditMessageAsync("msg1", newText, userId);

            result.Success.Should().BeFalse();
            Assert.Equal("Текст не может быть пустым", result.Error);
        }
        
        [Fact]
        public async Task EditMessage_TextTooLong_ReturnOperationResultFalse()
        {
            string userId = "user1";
            string longText = new string('a', 201);

            _mockUnitOfWork.Setup(u => u.Messages.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Message { UserId = userId });

            var result = await _messageService.EditMessageAsync("msg1", longText, userId);

            result.Success.Should().BeFalse();
            Assert.Equal("Сообщение слишком длинное", result.Error);
        }

        [Fact]
        public async Task EditMessage_MessageNotFound_ReturnOperationResultFalse()
        {
            _mockUnitOfWork.Setup(u => u.Messages.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Message)null);

            var result = await _messageService.EditMessageAsync("msg1", "newT", "user1");

            result.Success.Should().BeFalse();
            Assert.Equal("Сообщение не найдено", result.Error);
        }

        [Fact]
        public async Task EditMessage_ValidInput_UpdatesMessageAndSaves()
        {
            var message = new Message
            {
                Id = "msg1",
                UserId = "user1",
                Content = "old text"
            };

            _mockUnitOfWork.Setup(u => u.Messages.GetByIdAsync("msg1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(message);

            var result = await _messageService.EditMessageAsync("msg1", "new text", "user1");

            Assert.True(result.Success);
            Assert.Equal("new text", message.Content);
            _mockUnitOfWork.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task GetMessageAsync_WhenMessageNotFound_ThrowsArgumentNullException()
        {
            _mockUnitOfWork.Setup(u => u.Messages.GetMessageWithDetailsAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Message)null);

            await Assert.ThrowsAsync<ArgumentNullException>(() => _messageService.GetMessageAsync("msg1"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task LoadHistoryChatAsync_InvalidChatId_ThrowsArgumentException(string chatId)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
            _messageService.LoadHistoryChatAsync(chatId));
        }

        [Fact]
        public async Task LoadHistoryChatAsync_NegativeSkip_ThrowsArgumentOutOfRangeException()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _messageService.LoadHistoryChatAsync("chatId", skip: -1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public async Task LoadHistoryChatAsync_InvalidTake_ThrowsArgumentOutOfRangeException(int take)
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _messageService.LoadHistoryChatAsync("chatId", take: take));
        }

        [Fact]
        public async Task LoadHistoryChatAsync_ChatNotExists_ThrowsException()
        {
            _mockUnitOfWork.Setup(u => u.Chats.AnyAsync(
                It.IsAny<Expression<Func<Chat, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _messageService.LoadHistoryChatAsync("chatId"));
        }

        [Fact]
        public async Task LoadHistoryChatAsync_WithSkipAndTake_AppliesPagination()
        {
            var chatId = "valid-chat-id";
            var skip = 10;
            var take = 20;

            _mockUnitOfWork.Setup(u => u.Chats.AnyAsync(It.IsAny<Expression<Func<Chat, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var expectedMessages = new List<Message>();
            _mockUnitOfWork.Setup(u => u.Messages.LoadHistoryChatAsync(chatId, skip, take, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedMessages);

            object cachedValue = null;
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<string>(), out cachedValue))
                .Returns(false);

            _mockCache.Setup(c => c.CreateEntry(It.IsAny<string>()))
                .Returns(Mock.Of<ICacheEntry>);

            var result = await _messageService.LoadHistoryChatAsync(chatId, skip, take);

            _mockUnitOfWork.Verify(u => u.Messages.LoadHistoryChatAsync(chatId, skip, take, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task LoadHistoryChatAsync_ValidParameters_ReturnsMappedMessages()
        {
            var chatId = "chatId";
            var testMessages = new List<Message>
            {
                new Message { Id = "msg1", Content = "Hello" },
                new Message { Id = "msg2", Content = "World" }
            };

            _mockUnitOfWork.Setup(u => u.Chats.AnyAsync(It.IsAny<Expression<Func<Chat, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockUnitOfWork.Setup(u => u.Messages.LoadHistoryChatAsync(chatId, 0, 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testMessages);

            object cachedValue = null;
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<string>(), out cachedValue))
                .Returns(false);

            _mockCache.Setup(c => c.CreateEntry(It.IsAny<string>()))
                .Returns(Mock.Of<ICacheEntry>);

            var result = await _messageService.LoadHistoryChatAsync(chatId);

            result.Should().HaveCount(2);
            result.ElementAt(0).Id.Should().Be("msg1");
            result.ElementAt(0).Text.Should().Be("Hello");
            result.ElementAt(1).Id.Should().Be("msg2");
            result.ElementAt(1).Text.Should().Be("World");
            _mockUnitOfWork.Verify(u => u.Messages.LoadHistoryChatAsync(chatId, 0, 5, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteMessage_WhenMessageNotFound_ReturnOperationResultFalse()
        {
            _mockUnitOfWork.Setup(u => u.Messages.GetByIdAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Message)null);

            var result = await _messageService.DeleteMessageAsync("msg1", "user1");

            result.Success.Should().BeFalse();
            Assert.Equal("Сообщение не найдено", result.Error);
        }

        [Fact]
        public async Task DeleteMessage_WhenUserNotAuthorAndHasNotPermission_ReturnOperationResultFalse()
        {
            _mockUnitOfWork.Setup(u => u.Messages.GetByIdAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Message());

            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new User());

            _mockUserManager.Setup(u => u.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string>());

            var result = await _messageService.DeleteMessageAsync("msg1", "user1");

            result.Success.Should().BeFalse();
            Assert.Equal("Недостаточно прав", result.Error);
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("Mod")]
        public async Task DeleteMessage_WhenUserNotAuthorAndHasPermission_ReturnOperationResultTrue(string role)
        {
            string msgId = "msg1";
            string userId = "user1";

            _mockUnitOfWork.Setup(u => u.Messages.GetByIdAsync(
                msgId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Message());

            _mockUserManager.Setup(u => u.FindByIdAsync(userId))
                .ReturnsAsync(new User());

            _mockUserManager.Setup(u => u.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { role });

            var result = await _messageService.DeleteMessageAsync(msgId, userId);

            result.Success.Should().BeTrue();
            _mockUnitOfWork.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.Messages.DeleteAsync(msgId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("Mod")]
        [InlineData("User")]
        public async Task DeleteMessage_WhenUserAuthorAndHasPermission_ReturnOperationResultTrue(string role)
        {
            string userId = "user1";
            string msgId = "msg1";

            _mockUnitOfWork.Setup(u => u.Messages.GetByIdAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Message { UserId = userId });

            _mockUserManager.Setup(u => u.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { role });

            var result = await _messageService.DeleteMessageAsync(msgId, userId);

            result.Success.Should().BeTrue();
            _mockUnitOfWork.Verify(u => u.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.Messages.DeleteAsync(msgId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}