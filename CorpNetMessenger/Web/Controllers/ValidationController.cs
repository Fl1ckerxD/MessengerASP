using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CorpNetMessenger.Web.Controllers
{
    public class ValidationController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserContext _userContext;

        public ValidationController(UserManager<User> userManager, IUserContext userContext)
        {
            _userManager = userManager;
            _userContext = userContext;
        }

        [HttpGet]
        public async Task<IActionResult> IsEmailUnique(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var currentUserId = _userContext.UserId;

            // Если email свободен или принадлежит текущему пользователю - валидно
            bool isValid = user == null || user.Id == currentUserId;

            return new JsonResult(isValid);
        }

        [HttpGet]
        public async Task<IActionResult> IsUserNameUnique(string login)
        {
            var user = await _userManager.FindByNameAsync(login);
            return new JsonResult(user == null);
        }
    }
}
