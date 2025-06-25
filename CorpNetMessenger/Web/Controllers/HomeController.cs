using System.Diagnostics;
using CorpNetMessenger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CorpNetMessenger.Domain.Interfaces.Services;
using System.Security.Authentication;

namespace CorpNetMessenger.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IChatService _chatService;

        public HomeController(ILogger<HomeController> logger, IChatService chatService)
        {
            _logger = logger;
            _chatService = chatService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var chat = await _chatService.GetDepartmentChatForUserAsync(userId);

                return Redirect($"/messaging/chat/{chat.Id}");
            }
            catch (Exception ex) when (ex is AuthenticationException || ex is InvalidOperationException)
            {
                _logger.LogError(ex, "Ошибка перенаправления в чат");
                return BadRequest(ex.Message);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
