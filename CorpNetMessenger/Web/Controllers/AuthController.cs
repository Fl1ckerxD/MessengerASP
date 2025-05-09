using AutoMapper;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CorpNetMessenger.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAccountService _accountService;
        private readonly SignInManager<User> _signInManager;

        public AuthController(IMapper mapper, IAccountService accountService,
            SignInManager<User> signInManager)
        {
            _mapper = mapper;
            _accountService = accountService;
            _signInManager = signInManager;
        }

        public IActionResult Register() => View();
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if(!ModelState.IsValid) 
                return View(model);
            var result = await _accountService.Login(model);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if(!ModelState.IsValid)
                return View(model);
            try
            {
                var result = await _accountService.Register(model);

                if (result.Succeeded)
                {
                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                return NoContent();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
