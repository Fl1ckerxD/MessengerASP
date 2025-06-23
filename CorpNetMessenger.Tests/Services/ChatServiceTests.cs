using AutoMapper;
using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace CorpNetMessenger.Tests.Services
{
    public class ChatServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<ChatService>> _mockLogger;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly ChatService _chatService;

        public ChatServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<ChatService>>();
            _mockMapper = new Mock<IMapper>();

            var userStoreMock = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _chatService = new ChatService(
                _mockUnitOfWork.Object,
                _mockLogger.Object,
                _mockMapper.Object,
                _mockUserManager.Object);

            _mockUnitOfWork.Setup(u => u.ChatUsers.GetByPredicateAsync(
                It.IsAny<Expression<Func<ChatUser, bool>>>()))
                .ReturnsAsync(new ChatUser());
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

        [Fact]
        public async Task UserInChat_False_When_NotInChat()
        {
            _mockUnitOfWork.Setup(u => u.ChatUsers.GetByPredicateAsync(
                It.IsAny<Expression<Func<ChatUser, bool>>>()))
                .ReturnsAsync((ChatUser)null);

            var result = await _chatService.UserInChat("chat1", "user1");

            Assert.False(result);
        }
    }
}
