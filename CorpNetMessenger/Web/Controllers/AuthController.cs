using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CorpNetMessenger.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AuthController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public AuthController(IAccountService accountService, ILogger<AuthController> logger,
            IUnitOfWork unitOfWork)
        {
            _accountService = accountService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Login() => View();

        public async Task<IActionResult> Register()
        {
            var model = new RegisterViewModel();
            await LoadSelectListItem();

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountService.Login(model);

            if (result.Succeeded)
            {
                _logger.LogInformation("Пользователь {UserName} успешно вошел", model.UserName);
                return RedirectToAction("Index", "Home");
            }

            _logger.LogWarning("Неудачная попытка входа для {UserName}", model.UserName);
            ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectListItem();
                return View(model);
            }

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
                _logger.LogError(ex, "Ошибка регистрации пользователя");
                ModelState.AddModelError(string.Empty, "Произошла внутренняя ошибка. Попробуйте позже.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountService.Logout();
            return RedirectToAction("Index", "Home");
        }

        private async Task LoadSelectListItem()
        {
            var departments = await _unitOfWork.Departments.GetAllAsync();
            var posts = await _unitOfWork.Posts.GetAllAsync();

            ViewBag.Departments = departments.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Title
            });

            ViewBag.Posts = posts.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Title
            });
        }
    }
}
