using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
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
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly ChatService _chatService;
        private readonly Mock<IMemoryCache> _mockCache;

        public ChatServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<ChatService>>();
            _mockCache = new Mock<IMemoryCache>();

            var userStoreMock = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _chatService = new ChatService(
                _mockUnitOfWork.Object,
                _mockLogger.Object,
                _mockUserManager.Object,
                _mockCache.Object);

            _mockUnitOfWork.Setup(u => u.ChatUsers.GetByPredicateAsync(
                It.IsAny<Expression<Func<ChatUser, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ChatUser());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UserInChat_ReturnsCorrectResult(bool isUserInChat)
        {
            var mockChatUser = isUserInChat ? new ChatUser() : null;

            _mockUnitOfWork.Setup(u => u.ChatUsers.GetByPredicateAsync(
                It.IsAny<Expression<Func<ChatUser, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockChatUser);

            var result = await _chatService.UserInChatAsync("chat1", "user1");

            Assert.Equal(isUserInChat, result);
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

            _mockUnitOfWork.Setup(u => u.Chats.GetByDepartmentIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Chat)null);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _chatService.GetDepartmentChatForUserAsync("user1"));
        }

        [Fact]
        public async Task GetDepartmentChatForUserAsync_ReturnsCorrectResult()
        {
            _mockUserManager.Setup(u => u.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new User { DepartmentId = 1 });

            _mockUnitOfWork.Setup(u => u.Chats.GetByDepartmentIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Chat());

            var result = _chatService.GetDepartmentChatForUserAsync("user1");

            result.Should().NotBeNull();
        }
    }
}
