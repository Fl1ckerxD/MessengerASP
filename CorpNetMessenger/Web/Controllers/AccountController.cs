using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CorpNetMessenger.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Edit()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                throw new Exception("Пользователь не авторизован");

            var currentUser = await _unitOfWork.Users.GetByIdAsync(userId);
            var currentUserVM = new AccountViewModel
            {
                LastName = currentUser.LastName
            };
            return View();
        }
    }
}
