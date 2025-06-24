using CorpNetMessenger.Domain.Entities;
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
        private readonly ValidationController _controller;

        public ValidationControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _controller = new ValidationController(_userManagerMock.Object);
        }

        [Fact]
        public async Task IsEmailUnique_EmailNotExists_ReturnsTrue()
        {
            _userManagerMock.Setup(x => x.FindByEmailAsync("new@example.com"))
                .ReturnsAsync((User)null);

            SetupUserContext("userId");

            var result = await _controller.IsEmailUnique("new@example.com");

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.True((bool)jsonResult.Value);
        }

        [Fact]
        public async Task IsEmailUnique_EmailExistsButBelongsToCurrentUser_ReturnsTrue()
        {
            // Arrange
            var currentUserId = "currentUserId";
            var existingUser = new User { Id = currentUserId, Email = "existing@example.com" };

            _userManagerMock.Setup(x => x.FindByEmailAsync("existing@example.com"))
                .ReturnsAsync(existingUser);

            SetupUserContext(currentUserId);

            var result = await _controller.IsEmailUnique("existing@example.com");

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
