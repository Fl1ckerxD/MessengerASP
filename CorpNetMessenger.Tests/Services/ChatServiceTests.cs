using AutoMapper;
using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using System.Security.Authentication;

namespace CorpNetMessenger.Tests.Services
{
    public class ChatServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<ChatService>> _mockLogger;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly ChatService _chatService;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<IChatCacheService> _mockChatCacheService;

        public ChatServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<ChatService>>();
            _mockMapper = new Mock<IMapper>();
            _mockCache = new Mock<IMemoryCache>();
            _mockChatCacheService = new Mock<IChatCacheService>();

            var userStoreMock = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _chatService = new ChatService(
                _mockUnitOfWork.Object,
                _mockLogger.Object,
                _mockMapper.Object,
                _mockUserManager.Object,
                _mockCache.Object,
                _mockChatCacheService.Object);

            _mockUnitOfWork.Setup(u => u.ChatUsers.GetByPredicateAsync(
                It.IsAny<Expression<Func<ChatUser, bool>>>()))
                .ReturnsAsync(new ChatUser());
        }

        [Fact]
        public async Task SaveMessage_WhenRequestIsNull_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _chatService.SaveMessage(null, "user1"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task SaveMessage_WhenUserIdIsInvalid_ThrowsArgumentException(string? userId)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
            _chatService.SaveMessage(new(), userId));
        }

        [Fact]
        public async Task SaveMessage_WithValidData_SavesMessageAndReturnsId()
        {
            var request = new ChatMessageDto
            {
                ChatId = "chat1",
                Text = "test message",
                Files = new List<Attachment>()
            };
            var userId = "user1";
            var expectedMessageId = "msg123";

            _mockUnitOfWork.Setup(u => u.Messages.AddAsync(It.IsAny<Message>()))
                .Callback<Message>(msg => msg.Id = expectedMessageId);

            var result = await _chatService.SaveMessage(request, userId);

            result.Should().Be(expectedMessageId);
        }

        [Fact]
        public async Task SaveMessage_WithAttachments_SavesAllAttachments()
        {
            var attachments = new List<Attachment>
            {
                new Attachment { FileName = "file1.txt" },
                new Attachment { FileName = "file2.jpg" }
            };

            var request = new ChatMessageDto
            {
                ChatId = "chat1",
                Text = "test message",
                Files = attachments
            };

            Message savedMessage = null;
            _mockUnitOfWork.Setup(u => u.Messages.AddAsync(It.IsAny<Message>()))
                .Callback<Message>(msg => savedMessage = msg);

            await _chatService.SaveMessage(request, "123");

            savedMessage.Attachments.Should().HaveCount(2);
            savedMessage.Attachments.Should().BeEquivalentTo(attachments);
        }

        [Fact]
        public async Task SaveMessage_WhenUserNotInChat_ThrowsUnauthorizedAccessException()
        {
            var request = new ChatMessageDto { ChatId = "chat1" };
            var userId = "user1";

            _mockUnitOfWork.Setup(u => u.ChatUsers.GetByPredicateAsync(
                It.IsAny<Expression<Func<ChatUser, bool>>>()))
                .ReturnsAsync((ChatUser)null);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _chatService.SaveMessage(request, userId));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UserInChat_ReturnsCorrectResult(bool isUserInChat)
        {
            var mockChatUser = isUserInChat ? new ChatUser() : null;

            _mockUnitOfWork.Setup(u => u.ChatUsers.GetByPredicateAsync(
                It.IsAny<Expression<Func<ChatUser, bool>>>()))
                .ReturnsAsync(mockChatUser);

            var result = await _chatService.UserInChat("chat1", "user1");

            Assert.Equal(isUserInChat, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task EditMessage_WhenNewTextIsInvalid_ReturnOperationResultFalse(string? newText)
        {
            string userId = "user1";

            _mockUnitOfWork.Setup(u => u.Messages.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Message { UserId = userId });

            var result = await _chatService.EditMessage("msg1", newText, userId);

            result.Success.Should().BeFalse();
            Assert.Equal("Текст не может быть пустым", result.Error);
        }

        [Fact]
        public async Task EditMessage_TextTooLong_ReturnOperationResultFalse()
        {
            string userId = "user1";
            string longText = new string('a', 201);

            _mockUnitOfWork.Setup(u => u.Messages.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Message { UserId = userId });

            var result = await _chatService.EditMessage("msg1", longText, userId);

            result.Success.Should().BeFalse();
            Assert.Equal("Сообщение слишком длинное", result.Error);
        }

        [Fact]
        public async Task EditMessage_MessageNotFound_ReturnOperationResultFalse()
        {
            _mockUnitOfWork.Setup(u => u.Messages.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Message)null);

            var result = await _chatService.EditMessage("msg1", "newT", "user1");

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

            _mockUnitOfWork.Setup(u => u.Messages.GetByIdAsync("msg1"))
                .ReturnsAsync(message);

            var result = await _chatService.EditMessage("msg1", "new text", "user1");

            Assert.True(result.Success);
            Assert.Equal("new text", message.Content);
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once());
        }

        [Fact]
        public async Task GetMessageAsync_WhenMessageNotFound_ThrowsArgumentNullException()
        {
            _mockUnitOfWork.Setup(u => u.Messages.GetMessageWithDetailsAsync(
                It.IsAny<string>()))
                .ReturnsAsync((Message)null);

            await Assert.ThrowsAsync<ArgumentNullException>(() => _chatService.GetMessageAsync("msg1"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task LoadHistoryChatAsync_InvalidChatId_ThrowsArgumentException(string chatId)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
            _chatService.LoadHistoryChatAsync(chatId));
        }

        [Fact]
        public async Task LoadHistoryChatAsync_NegativeSkip_ThrowsArgumentOutOfRangeException()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _chatService.LoadHistoryChatAsync("chatId", skip: -1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public async Task LoadHistoryChatAsync_InvalidTake_ThrowsArgumentOutOfRangeException(int take)
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _chatService.LoadHistoryChatAsync("chatId", take: take));
        }

        [Fact]
        public async Task LoadHistoryChatAsync_ChatNotExists_ThrowsException()
        {
            _mockUnitOfWork.Setup(u => u.Chats.AnyAsync(
                It.IsAny<Expression<Func<Chat, bool>>>()))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<Exception>(() =>
                _chatService.LoadHistoryChatAsync("chatId"));
        }

        [Fact]
        public async Task LoadHistoryChatAsync_WithSkipAndTake_AppliesPagination()
        {
            var chatId = "valid-chat-id";
            var skip = 10;
            var take = 20;

            _mockUnitOfWork.Setup(u => u.Chats.AnyAsync(It.IsAny<Expression<Func<Chat, bool>>>()))
                .ReturnsAsync(true);

            var expectedMessages = new List<Message>();
            _mockUnitOfWork.Setup(u => u.Messages.LoadHistoryChatAsync(chatId, skip, take))
                .ReturnsAsync(expectedMessages);

            object cachedValue = null;
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<string>(), out cachedValue))
                .Returns(false);

            _mockCache.Setup(c => c.CreateEntry(It.IsAny<string>()))
                .Returns(Mock.Of<ICacheEntry>);

            var result = await _chatService.LoadHistoryChatAsync(chatId, skip, take);

            _mockUnitOfWork.Verify(u => u.Messages.LoadHistoryChatAsync(chatId, skip, take), Times.Once);
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

            var expectedDtos = new List<MessageDto>
            {
                new MessageDto { Id = "msg1", Text = "Hello" },
                new MessageDto { Id = "msg2", Text = "World" }
            };

            _mockUnitOfWork.Setup(u => u.Chats.AnyAsync(It.IsAny<Expression<Func<Chat, bool>>>()))
                .ReturnsAsync(true);

            _mockUnitOfWork.Setup(u => u.Messages.LoadHistoryChatAsync(chatId, 0, 5))
                .ReturnsAsync(testMessages);

            _mockMapper.Setup(m => m.Map<List<MessageDto>>(testMessages))
                .Returns(expectedDtos);

            object cachedValue = null;
            _mockCache.Setup(c => c.TryGetValue(It.IsAny<string>(), out cachedValue))
                .Returns(false);

            _mockCache.Setup(c => c.CreateEntry(It.IsAny<string>()))
                .Returns(Mock.Of<ICacheEntry>);

            var result = await _chatService.LoadHistoryChatAsync(chatId);

            result.Should().BeEquivalentTo(expectedDtos);
            _mockUnitOfWork.Verify(u => u.Messages.LoadHistoryChatAsync(chatId, 0, 5), Times.Once);
        }

        [Fact]
        public async Task DeleteMessage_WhenMessageNotFound_ReturnOperationResultFalse()
        {
            _mockUnitOfWork.Setup(u => u.Messages.GetByIdAsync(
                It.IsAny<string>()))
                .ReturnsAsync((Message)null);

            var result = await _chatService.DeleteMessage("msg1", "user1");

            result.Success.Should().BeFalse();
            Assert.Equal("Сообщение не найдено", result.Error);
        }

        [Fact]
        public async Task DeleteMessage_WhenUserNotAuthorAndHasNotPermission_ReturnOperationResultFalse()
        {
            _mockUnitOfWork.Setup(u => u.Messages.GetByIdAsync(
                It.IsAny<string>()))
                .ReturnsAsync(new Message());

            _mockUserManager.Setup(u => u.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string>());

            var result = await _chatService.DeleteMessage("msg1", "user1");

            result.Success.Should().BeFalse();
            Assert.Equal("Недостаточно прав", result.Error);
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("Mod")]
        public async Task DeleteMessage_WhenUserNotAuthorAndHasPermission_ReturnOperationResultTrue(string role)
        {
            string msgId = "msg1";

            _mockUnitOfWork.Setup(u => u.Messages.GetByIdAsync(
                It.IsAny<string>()))
                .ReturnsAsync(new Message());

            _mockUserManager.Setup(u => u.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { role });

            var result = await _chatService.DeleteMessage("msg1", "user1");

            result.Success.Should().BeTrue();
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.Messages.DeleteAsync(msgId), Times.Once);
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
                It.IsAny<string>()))
                .ReturnsAsync(new Message { UserId = userId });

            _mockUserManager.Setup(u => u.GetRolesAsync(It.IsAny<User>()))
                .ReturnsAsync(new List<string> { role });

            var result = await _chatService.DeleteMessage(msgId, userId);

            result.Success.Should().BeTrue();
            _mockUnitOfWork.Verify(u => u.SaveAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.Messages.DeleteAsync(msgId), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetDepartmentChatForUserAsync_InvalidUserId_ThrowsArgumentException(string userId)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
            _chatService.GetDepartmentChatForUserAsync(userId));
        }

        [Fact]
        public async Task GetDepartmentChatForUserAsync_WhenUserNotFound_ThrowsAuthenticationException()
        {
            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            await Assert.ThrowsAsync<AuthenticationException>(() =>
            _chatService.GetDepartmentChatForUserAsync("user1"));
        }

        [Fact]
        public async Task GetDepartmentChatForUserAsync_InvalidDepartmentId_ThrowsInvalidOperationException()
        {
            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new User());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _chatService.GetDepartmentChatForUserAsync("user1"));
        }

        [Fact]
        public async Task GetDepartmentChatForUserAsync_WhenChatNotFound_ThrowsArgumentNullException()
        {
            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { DepartmentId = 1 });

            _mockUnitOfWork.Setup(u => u.Chats.GetByDepartmentIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Chat)null);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _chatService.GetDepartmentChatForUserAsync("user1"));
        }

        [Fact]
        public async Task GetDepartmentChatForUserAsync_ReturnsCorrectResult()
        {
            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { DepartmentId = 1 });

            _mockUnitOfWork.Setup(u => u.Chats.GetByDepartmentIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Chat());

            var result = _chatService.GetDepartmentChatForUserAsync("user1");

            result.Should().NotBeNull();
        }
    }
}
