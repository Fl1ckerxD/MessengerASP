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
        private readonly IUserContext _userContext;

        public HomeController(ILogger<HomeController> logger, IChatService chatService, IUserContext userContext)
        {
            _logger = logger;
            _chatService = chatService;
            _userContext = userContext;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                var chat = await _chatService.GetDepartmentChatForUserAsync(_userContext.UserId, cancellationToken);
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
