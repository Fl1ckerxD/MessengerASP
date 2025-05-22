using AutoMapper;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Infrastructure.Services;
using CorpNetMessenger.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace CorpNetMessenger.Tests.Services
{
    public class AccountServiceTests
    {
        private Mock<UserManager<User>> _userManagerMock;
        private Mock<SignInManager<User>> _signInManagerMock;
        public AccountServiceTests()
        {
            var userStore = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);
            _signInManagerMock = new Mock<SignInManager<User>>(_userManagerMock.Object, Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<User>>(), null, null, null, null);
        }

        [Fact]
        public async Task Login_CallsPasswordSignInAsync_WithCorrectParameters()
        {
            var model = new LoginViewModel { UserName = "test", Password = "password", RememberMe = true };

            var accountService = new AccountService(Mock.Of<IMapper>(), _userManagerMock.Object, _signInManagerMock.Object, Mock.Of<ILogger<AccountService>>());

            await accountService.Login(model);

            _signInManagerMock.Verify(x => x.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false), Times.Once);
        }
    }
}
