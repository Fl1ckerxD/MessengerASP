using System.Security.Claims;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorpNetMessenger.Web.Areas.Messaging.Controllers
{
    [Area("Messaging")]
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatService _chatService;
        private readonly IEmployeeService _employeeService;

        public ChatController(
            ILogger<ChatController> logger,
            IUnitOfWork unitOfWork,
            IChatService chatService,
            IEmployeeService employeeService
        )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _chatService = chatService;
            _employeeService = employeeService;
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
            string currentUserId =
                user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User not authenticated");
            ;

            bool isInChat = await _chatService.UserInChat(id, currentUserId);

            if (!isInChat)
            {
                _logger.LogWarning(
                    "Пользователь {UserId} не состоит в чате {ChatId}",
                    currentUserId,
                    id
                );
                return Forbid();
            }

            try
            {
                var messages = await _chatService.LoadHistoryChatAsync(id, take: 20);
                var contacts = await _unitOfWork.Users.GetAllDepartmentContactsAsync(currentUserId);
                var ChatName = (await _unitOfWork.Chats.GetByIdAsync(id)).Name;

                var currentUser = contacts.FirstOrDefault(u => u.Id == currentUserId);
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
                    CurrentUser = currentUser,
                };

                var model = new ChatViewModel { Contacts = contactVM, Chat = messages.Reverse() };

                ViewBag.ChatName = ChatName;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка при загрузке чата: {ChatId}, User: {UserId}",
                    id,
                    currentUserId
                );
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> SearchEmployees(string term)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var employees = await _employeeService.SearchEmployees(term, user.DepartmentId.Value, user.Id);
            return PartialView("_EmployeeListPartial", employees);
        }
    }
}
