using System.Security.Claims;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CorpNetMessenger.Web.Areas.Messaging.Controllers
{
    [Area("Messaging")]
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatService _chatService;
        private readonly IMessageService _messageService;
        private readonly IEmployeeService _employeeService;
        private readonly IMemoryCache _cache;
        private readonly IUserContext _userContext;

        public ChatController(
            ILogger<ChatController> logger,
            IUnitOfWork unitOfWork,
            IChatService chatService,
            IEmployeeService employeeService,
            IMemoryCache cache,
            IMessageService messageService,
            IUserContext userContext
        )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _chatService = chatService;
            _employeeService = employeeService;
            _cache = cache;
            _messageService = messageService;
            _userContext = userContext;
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
            string currentUserId = _userContext.UserId;
            bool isInChat = await _chatService.UserInChatAsync(id, currentUserId);

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
                var messages = await _messageService.LoadHistoryChatAsync(id, take: 20);
                var ChatName = (await _unitOfWork.Chats.GetByIdAsync(id)).Name;

                var cacheContactsKey = $"contacts_chat_{id}";
                var contacts = await _cache.GetOrCreateAsync(cacheContactsKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return await _unitOfWork.Users.GetAllDepartmentContactsAsync(currentUserId); 
                });

                var filteredContacts = contacts.Where(u => u.Id != currentUserId).ToList();

                var currentUser = contacts.FirstOrDefault(u => u.Id == currentUserId);

                if (currentUser == null)
                {
                    _logger.LogWarning("Текущий пользователь не найден в списке контактов");
                }

                var contactVM = new ContactPanelViewModel
                {
                    Contacts = filteredContacts,
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
            var user = await _unitOfWork.Users.GetByIdAsync(_userContext.UserId);
            var employees = await _employeeService.SearchEmployeesAsync(term, user.DepartmentId.Value, user.Id);
            return PartialView("_EmployeeListPartial", employees);
        }
    }
}
