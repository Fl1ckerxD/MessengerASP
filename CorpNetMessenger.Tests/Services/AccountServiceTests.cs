using AutoMapper;
using CorpNetMessenger.Application.Configs;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Infrastructure.Services;
using CorpNetMessenger.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace CorpNetMessenger.Tests.Services
{
    public class AccountServiceTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<ILogger<AccountService>> _loggerMock;
        private readonly AccountService _accountService;

        public AccountServiceTests()
        {
            _mapperMock = new Mock<IMapper>();
            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<User>>(),
                null, null, null, null);
            _loggerMock = new Mock<ILogger<AccountService>>();

            _accountService = new AccountService(
                _mapperMock.Object,
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Login_Successful_UpdatesFullNameClaim()
        {
            var model = new LoginViewModel
            {
                UserName = "testUser",
                Password = "Test123!",
                RememberMe = false
            };

            var user = new User { UserName = "testUser", Name = "John", LastName = "Doe", StatusId = StatusTypes.Active };

            _signInManagerMock.Setup(x => x.PasswordSignInAsync(
                model.UserName, model.Password, model.RememberMe, false))
                .ReturnsAsync(SignInResult.Success);

            _userManagerMock.Setup(x => x.FindByNameAsync(model.UserName))
                .ReturnsAsync(user);

            _userManagerMock.Setup(x => x.GetClaimsAsync(user))
                .ReturnsAsync(new List<Claim>());

            var result = await _accountService.LoginAsync(model);

            Assert.Equal(SignInResult.Success, result);
            _userManagerMock.Verify(x => x.AddClaimAsync(
                user, It.Is<Claim>(c => c.Type == "FullName" && c.Value == "Doe John")),
                Times.Once);
        }

        [Fact]
        public async Task Login_Failed_DoesNotUpdateClaims()
        {
            var model = new LoginViewModel
            {
                UserName = "testUser",
                Password = "WrongPassword",
                RememberMe = false
            };

            _signInManagerMock.Setup(x => x.PasswordSignInAsync(
                model.UserName, model.Password, model.RememberMe, false))
                .ReturnsAsync(SignInResult.Failed);

            var result = await _accountService.LoginAsync(model);

            Assert.Equal(SignInResult.Failed, result);
            _userManagerMock.Verify(x => x.AddClaimAsync(It.IsAny<User>(), It.IsAny<Claim>()), Times.Never);
        }

        [Fact]
        public async Task Login_NotAllowed_UserStatusIsPending()
        {
            var model = new LoginViewModel
            {
                UserName = "testUser",
                Password = "Password",
                RememberMe = false
            };

            var user = new User { StatusId = StatusTypes.Pending };

            _userManagerMock.Setup(x => x.FindByNameAsync(model.UserName))
                .ReturnsAsync(user);

            var result = await _accountService.LoginAsync(model);

            Assert.Equal(SignInResult.NotAllowed, result);
            _signInManagerMock.Verify(x => x.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false), Times.Never);
        }

        [Fact]
        public async Task Register_Successful_CreatesUser()
        {
            var model = new RegisterViewModel
            {
                Login = "newUser",
                Email = "new@example.com",
                Password = "Test123!",
                Name = "John",
                LastName = "Doe"
            };

            var user = new User { UserName = model.Login, Email = model.Email };

            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync((User)null);

            _mapperMock.Setup(x => x.Map<User>(model))
                .Returns(user);

            _userManagerMock.Setup(x => x.CreateAsync(user, model.Password))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _accountService.RegisterAsync(model);

            Assert.True(result.Succeeded);
            _userManagerMock.Verify(x => x.CreateAsync(user, model.Password), Times.Once);
        }

        [Fact]
        public async Task Register_EmailExists_ReturnsError()
        {
            var model = new RegisterViewModel
            {
                Login = "newUser",
                Email = "existing@example.com",
                Password = "Test123!"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(model.Email))
                .ReturnsAsync(new User());

            var result = await _accountService.RegisterAsync(model);

            Assert.False(result.Succeeded);
            Assert.Contains("Такая почта уже используется", result.Errors.First().Description);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Logout_CallsSignOutAsync()
        {
            await _accountService.LogoutAsync();

            _signInManagerMock.Verify(x => x.SignOutAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateFullNameClaim_ExistingClaimDifferentValue_ReplacesClaim()
        {
            var user = new User { Name = "John", LastName = "Doe" };
            var oldClaim = new Claim("FullName", "Old Name");

            _userManagerMock.Setup(x => x.GetClaimsAsync(user))
                .ReturnsAsync(new List<Claim> { oldClaim });

            await _accountService.UpdateFullNameClaim(user);

            _userManagerMock.Verify(x => x.ReplaceClaimAsync(
                user,
                oldClaim,
                It.Is<Claim>(c => c.Type == "FullName" && c.Value == "Doe John")),
                Times.Once);
        }

        [Fact]
        public async Task UpdateFullNameClaim_NoExistingClaim_AddsNewClaim()
        {
            var user = new User { Name = "John", LastName = "Doe" };

            _userManagerMock.Setup(x => x.GetClaimsAsync(user))
                .ReturnsAsync(new List<Claim>());

            await _accountService.UpdateFullNameClaim(user);

            _userManagerMock.Verify(x => x.AddClaimAsync(
                user,
                It.Is<Claim>(c => c.Type == "FullName" && c.Value == "Doe John")),
                Times.Once);
        }
    }
}
