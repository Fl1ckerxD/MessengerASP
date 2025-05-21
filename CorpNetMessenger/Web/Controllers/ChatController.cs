using AutoMapper.Execution;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CorpNetMessenger.Web.Controllers
{
    public class ChatController : Controller
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatService _chatService;

        public ChatController(ILogger<ChatController> logger, IUnitOfWork unitOfWork,
            IChatService chatService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _chatService = chatService;
        }

        public async Task<IActionResult> Index(string id)
        {
            var user = HttpContext.User;
            string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            bool isInChat = await _chatService.UserInChat(id, userId);

            if (!isInChat)
            {
                _logger.LogWarning("Попытка доступа к чату без прав: {ChatId}, User: {UserId}", id, userId);
                return Forbid();
            }

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
