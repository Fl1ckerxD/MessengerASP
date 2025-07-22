using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace CorpNetMessenger.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AuthController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public AuthController(IAccountService accountService, ILogger<AuthController> logger,
            IUnitOfWork unitOfWork, IMemoryCache cache)
        {
            _accountService = accountService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public IActionResult Login() => View();

        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Register()
        {
            var model = new RegisterViewModel();
            await LoadSelectLists();
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
                _logger.LogInformation("Успешный вход: {UserName}", model.UserName);
                return RedirectToAction("Index", "Home");
            }

            else if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Вход в систему невозможен.");
                return View(model);
            }

            _logger.LogWarning("Неудачный вход: {UserName}", model.UserName);
            ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadSelectLists();
                    return View(model);
                }

                var result = await _accountService.Register(model);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Новый пользователь зарегистрирован: {Email}", model.Email);
                    return RedirectToAction("Login");
                }

                await LoadSelectLists();
                AddModelErrors(result);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка регистрации: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Произошла внутренняя ошибка. Попробуйте позже.");
                await LoadSelectLists();
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

        private async Task LoadSelectLists()
        {
            var departments = await _cache.GetOrCreateAsync("departments", async entry => {
                entry.AbsoluteExpiration = DateTime.Now.AddHours(6);
                return await _unitOfWork.Departments.GetAllAsync();
            });

            var posts = await _cache.GetOrCreateAsync("posts", async entry => {
                entry.AbsoluteExpiration = DateTime.Now.AddHours(6);
                return await _unitOfWork.Posts.GetAllAsync();
            });

            ViewBag.Departments = departments.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Title
            }).ToList();

            ViewBag.Posts = posts.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Title
            }).ToList();
        }

        private void AddModelErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                var key = error.Code switch
                {
                    var s when s.Contains("Password") => nameof(RegisterViewModel.Password),
                    var s when s.Contains("Email") => nameof(RegisterViewModel.Email),
                    var s when s.Contains("UserName") => nameof(RegisterViewModel.Login),
                    _ => string.Empty
                };

                ModelState.AddModelError(key, error.Description);
            }
        }
    }
}
