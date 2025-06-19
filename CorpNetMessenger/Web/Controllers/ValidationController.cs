using CorpNetMessenger.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CorpNetMessenger.Web.Controllers
{
    public class ValidationController : Controller
    {
        private readonly UserManager<User> _userManager;

        public ValidationController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> IsEmailUnique(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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
