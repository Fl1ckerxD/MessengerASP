using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Infrastructure.Services;
using CorpNetMessenger.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace CorpNetMessenger.Tests.Controllers
{
    public class ValidationControllerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IUserContext> _userContextMock;
        private readonly ValidationController _controller;

        public ValidationControllerTests()
        {
            _userContextMock = new Mock<IUserContext>();
            _userContextMock.Setup(uc => uc.UserId).Returns("testUserId");

            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _controller = new ValidationController(_userManagerMock.Object, _userContextMock.Object);
        }

        [Fact]
        public async Task IsEmailUnique_EmailNotExists_ReturnsTrue()
        {
            string email = "new@example.com";
            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((User)null);

            SetupUserContext("userId");

            var result = await _controller.IsEmailUnique(email);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.True((bool)jsonResult.Value);
        }

        [Fact]
        public async Task IsEmailUnique_EmailExistsButBelongsToCurrentUser_ReturnsTrue()
        {
            // Arrange
            var currentUserId = "testUserId";
            var existingUser = new User { Id = currentUserId, Email = "existing@example.com" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(existingUser.Email))
                .ReturnsAsync(existingUser);

            SetupUserContext(currentUserId);

            var result = await _controller.IsEmailUnique(existingUser.Email);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.True((bool)jsonResult.Value);
        }

        [Fact]
        public async Task IsEmailUnique_EmailExistsAndBelongsToOtherUser_ReturnsFalse()
        {
            var existingUser = new User { Id = "userId", Email = "existing@example.com" };

            _userManagerMock.Setup(x => x.FindByEmailAsync("existing@example.com"))
                .ReturnsAsync(existingUser);

            SetupUserContext("currentUserId");

            var result = await _controller.IsEmailUnique("existing@example.com");

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.False((bool)jsonResult.Value);
        }

        [Fact]
        public async Task IsUserNameUnique_UserNameNotExists_ReturnsTrue()
        {
            _userManagerMock.Setup(x => x.FindByNameAsync("newuser"))
                .ReturnsAsync((User)null);

            var result = await _controller.IsUserNameUnique("newuser");

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.True((bool)jsonResult.Value);
        }

        [Fact]
        public async Task IsUserNameUnique_UserNameExists_ReturnsFalse()
        {
            _userManagerMock.Setup(x => x.FindByNameAsync("existinguser"))
                .ReturnsAsync(new User());

            var result = await _controller.IsUserNameUnique("existinguser");

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.False((bool)jsonResult.Value);
        }

        private void SetupUserContext(string userId)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }
    }
}
