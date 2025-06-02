using AutoMapper.Execution;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CorpNetMessenger.Web.Areas.Messaging.Controllers
{
    [Area("Messaging")]
    [Authorize]
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
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("Попытка открыть чат с пустым ID");
                return BadRequest("ID чата не указан");
            }

            var chatExists = await _unitOfWork.Chats.AnyAsync(c => c.Id == id);
            if (!chatExists)
            {
                _logger.LogWarning("Чат {ChatId} не найден", id);
                return NotFound();
            }

            var user = HttpContext.User;
            string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            bool isInChat = await _chatService.UserInChat(id, userId);

            if (!isInChat)
            {
                _logger.LogWarning("Пользователь {UserId} не состоит в чате {ChatId}", userId, id);
                return Forbid();
            }

            try
            {
                var messages = await _unitOfWork.Messages.LoadHistoryChatAsync(id);
                var contacts = await _unitOfWork.Users.GetAllDepartmentContactsAsync(userId);

                var currentUser = contacts.FirstOrDefault(u => u.Id == userId);
                if (currentUser != null)
                {
                    contacts.Remove(currentUser);
                }
                else
                {
                    _logger.LogWarning("Текущий пользователь не найден в списке контактов");
                }

                var contactVM = new ContactPanelViewModel
                {
                    Contacts = contacts,
                    CurrentUser = currentUser
                };

                var model = new ChatViewModel
                {
                    Contacts = contactVM,
                    Chat = messages.Reverse()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке чата: {ChatId}, User: {UserId}", id, userId);
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
