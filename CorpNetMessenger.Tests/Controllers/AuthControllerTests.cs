using Castle.Core.Logging;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.Controllers;
using CorpNetMessenger.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CorpNetMessenger.Tests.Controllers
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Login_Post_SuccessfulLogin_RedirectsToHome()
        {
            var accountServiceMock = new Mock<IAccountService>();
            accountServiceMock.Setup(x => x.Login(It.IsAny<LoginViewModel>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var controller = new AuthController(accountServiceMock.Object, Mock.Of<ILogger<AuthController>>());
            var model = new LoginViewModel();

            var result = await controller.Login(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_Post_FailedLogin_AddsErrorAndView()
        {
            var accountServiceMock = new Mock<IAccountService>();
            accountServiceMock.Setup(x => x.Login(It.IsAny<LoginViewModel>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var controller = new AuthController(accountServiceMock.Object, Mock.Of<ILogger<AuthController>>());
            var model = new LoginViewModel();
            
            var result = await controller.Login(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
            Assert.True(controller.ModelState.ErrorCount > 0);
        }

        [Fact]
        public async Task Register_Post_SuccessfulRegistration_RedirectsToLogin()
        {
            var accountServiceMock = new Mock<IAccountService>();
            accountServiceMock.Setup(x => x.Register(It.IsAny<RegisterViewModel>()))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new AuthController(accountServiceMock.Object, Mock.Of<ILogger<AuthController>>());
            var model = new RegisterViewModel();

            var result = await controller.Register(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        [Fact]
        public async Task Register_Post_HasErrors_AddsModelErrorsAndView()
        {
            var errors = new[] { new IdentityError { Description = "Error1" }, new IdentityError { Description = "Error2" } };
            var accountServiceMock = new Mock<IAccountService>();
            accountServiceMock.Setup(x => x.Register(It.IsAny<RegisterViewModel>()))
                .ReturnsAsync(IdentityResult.Failed(errors));

            var controller = new AuthController(accountServiceMock.Object, Mock.Of<ILogger<AuthController>>());
            var model = new RegisterViewModel();

            var result = await controller.Register(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(model, viewResult.Model);
            Assert.Equal(2, controller.ModelState.ErrorCount);
        }
    }
}
