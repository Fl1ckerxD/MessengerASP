using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Authentication;

namespace CorpNetMessenger.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ChatService> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _cache;
        public ChatService(IUnitOfWork unitOfWork, ILogger<ChatService> logger,
            UserManager<User> userManager, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _cache = cache;
        }    

        /// <summary>
        /// Проверка состоит ли пользователь в указанном чате
        /// </summary>
        /// <param name="chatId">Id группы</param>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Возвращает true если состоит иначе false</returns>
        public async Task<bool> UserInChat(string chatId, string userId)
        {
            var chatUser = await _unitOfWork.ChatUsers.GetByPredicateAsync(cu => cu.ChatId == chatId && cu.UserId == userId);
            return chatUser != null;
        }

        /// <summary>
        /// Возвращает чат отдела для указанного пользователя
        /// </summary>
        /// <exception cref="AuthenticationException">Когда пользователь или чат не найдены</exception>
        /// <exception cref="InvalidOperationException">Когда у пользователя не назначен отдел</exception>
        public async Task<Chat> GetDepartmentChatForUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("Идентификатор пользователя не может быть пустым", nameof(userId));

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("Пользователь {UserId} не найден", userId);
                throw new AuthenticationException($"Пользователь {userId} не найдент");
            }

            if (user.DepartmentId == null)
            {
                _logger.LogWarning("Для пользователя {UserId} не назначен отдел", userId);
                throw new InvalidOperationException($"Пользователь {userId} не имеет назначенного отдела");
            }

            var chat = await _unitOfWork.Chats.GetByDepartmentIdAsync(user.DepartmentId.Value);

            if (chat == null)
            {
                _logger.LogWarning("Чат для отдела {DepartmentId} не найден", user.DepartmentId.Value);
                throw new ArgumentNullException($"Чат отдела {user.DepartmentId.Value} не найден");
            }

            return chat;
        }

        /// <summary>
        /// Добавляет пользователя в чат по идентификаторам
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="chatId">Идентификатор чата</param>
        public async Task AddUserToChat(string userId, string chatId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId), "User ID не может быть пустым");
            if (string.IsNullOrEmpty(chatId))
                throw new ArgumentNullException(nameof(chatId), "Chat ID не может быть пустым");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            await AddUserToChat(user, chatId);
        }

        /// <summary>
        /// Основной метод добавления пользователя в чат
        /// </summary>
        /// <param name="user">Объект пользователя</param>
        /// <param name="chatId">Идентификатор чата</param>
        public async Task AddUserToChat(User user, string chatId)
        {
            ArgumentNullException.ThrowIfNull(user);
            if (string.IsNullOrEmpty(chatId))
                throw new ArgumentNullException(nameof(chatId), "Chat ID не может быть пустым");

            try
            {
                // Проверка существования чата
                var chatExists = await _unitOfWork.Chats.AnyAsync(c => c.Id == chatId);
                if (!chatExists)
                    throw new InvalidOperationException($"Чата с ID {chatId} не существует");

                // Проверка, что пользователь еще не в чате
                var isUserInChat = await UserInChat(chatId, user.Id);
                if (isUserInChat)
                    throw new InvalidOperationException("Пользователь уже в этом чате");

                ChatUser chatUser = new() { ChatId = chatId, UserId = user.Id };
                user.Chats.Add(chatUser);
                await _unitOfWork.SaveAsync();     
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении пользователя {UserId} в чат {ChatId}: {Message}", user.Id, chatId, ex.Message);
                throw;
            }

            var cacheContactsKey = $"contacts_chat_{chatId}";
            _cache.Remove(cacheContactsKey);
        }

        /// <summary>
        /// Добавляет пользователя в чат его отдела
        /// </summary>
        /// <param name="user">Объект пользователя</param>
        public async Task AddUserToDepartmentChat(User user)
        {
            ArgumentNullException.ThrowIfNull(user);
            if (user.DepartmentId == null)
                throw new InvalidOperationException("У пользователя не назначен отдел");

            var chat = await _unitOfWork.Chats.GetByDepartmentIdAsync(user.DepartmentId.Value);
            if (chat == null)
                throw new InvalidOperationException("Чат отдела не найден");
            await AddUserToChat(user, chat.Id);
        }
    }
}
