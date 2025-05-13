using CorpNetMessenger.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CorpNetMessenger.Web.Controllers
{
    public class ChatController : Controller
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ChatController(ILogger<ChatController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(string id)
        {
            try
            {
                var messages = await _unitOfWork.Messages.GetChatMessagesAsync(id);
                return View(messages);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "перенаправление на главную страницу");
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
