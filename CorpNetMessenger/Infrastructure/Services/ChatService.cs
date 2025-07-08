using AutoMapper;
using CorpNetMessenger.Application.Common;
using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using System.Security.Authentication;

namespace CorpNetMessenger.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ChatService> _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IMemoryCache _cache;
        public ChatService(IUnitOfWork unitOfWork, ILogger<ChatService> logger,
            IMapper mapper, UserManager<User> userManager, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _cache = cache;
        }

        /// <summary>
        /// Сохранение сообщения в БД
        /// </summary>
        /// <param name="content">Текст сообщения</param>
        /// <param name="userId">Id пользователя отправившего сообщение</param>
        /// <param name="chatId">Чат в который было отправлено сообщение</param>
        /// <returns>Возвращает Id сохраненного сообщения</returns>
        public async Task<string> SaveMessage(ChatMessageDto request, string userId)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID не может быть пустым", nameof(userId));

            try
            {
                bool chatUser = await UserInChat(request.ChatId, userId);

                if (!chatUser)
                    throw new UnauthorizedAccessException("Пользователь не состоит в чате");

                var message = new Message
                {
                    ChatId = request.ChatId,
                    Content = request.Text != null ? request.Text.Trim() : "",
                    UserId = userId,
                    Attachments = request.Files
                };

                await _unitOfWork.Messages.AddAsync(message);
                await _unitOfWork.SaveAsync();

                return message.Id;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogInformation(ex, "Пользователь {UserId} попытался отправить сообщение в чат {ChatId}",
                   userId, request?.ChatId);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении сообщения в чате {ChatId} пользователем {UserId}",
                    request?.ChatId, userId);
                throw new Exception("Ошибка сохранения сообщения", ex);
            }
        }

        /// <summary>
        /// Редактирование сообщения
        /// </summary>
        /// <param name="messageId">Id конкретного сообщения для редактирования</param>
        /// <param name="newText">Отредактированный текст</param>
        /// <returns>Возвращает true если сообщение было отредактировано иначе false</returns>
        public async Task<OperationResult> EditMessage(string messageId, string newText, string userId)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);

            if (message == null)
                return new OperationResult { Success = false, Error = "Сообщение не найдено" };

            if (message.UserId != userId)
                return new OperationResult { Success = false, Error = "Нельзя редактировать чужое сообщение" };

            if (string.IsNullOrWhiteSpace(newText))
                return new OperationResult { Success = false, Error = "Текст не может быть пустым" };

            if (newText.Length > 200)
                return new OperationResult { Success = false, Error = "Сообщение слишком длинное" };

            message.Content = newText;
            await _unitOfWork.SaveAsync();

            return new OperationResult { Success = true };
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
        /// Получает сообщение по его идентификатору и преобразует в DTO для передачи клиенту.
        /// </summary>
        /// <param name="messageId">Идентификатор сообщения</param>
        /// <returns>Объект <see cref="MessageDto"/>, представляющий сообщение с дополнительными данными</returns>
        /// <exception cref="ArgumentNullException">Если сообщение с указанным ID не найдено</exception>
        public async Task<MessageDto> GetMessageAsync(string messageId)
        {
            var messageEntity = await _unitOfWork.Messages.GetMessageWithDetailsAsync(messageId);
            if (messageEntity == null)
                throw new ArgumentNullException(nameof(messageId), "Сообщение не найдено");

            var messageDto = _mapper.Map<MessageDto>(messageEntity);
            return messageDto;
        }

        public async Task<IEnumerable<MessageDto>> LoadHistoryChatAsync(string chatId, int skip = 0, int take = 5)
        {
            if (string.IsNullOrWhiteSpace(chatId))
                throw new ArgumentException("Chat ID не может быть пустым", nameof(chatId));

            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));
            if (take < 1 || take > 100) throw new ArgumentOutOfRangeException(nameof(take));

            if (!await _unitOfWork.Chats.AnyAsync(c => c.Id == chatId)) // Проверка наличия чата с таким id
                throw new Exception("Такого чата нет");

            var messages = await _unitOfWork.Messages.LoadHistoryChatAsync(chatId, skip, take);
            return _mapper.Map<List<MessageDto>>(messages);
        }

        public async Task<OperationResult> DeleteMessage(string messageId, string userId)
        {
            try
            {
                var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
                if (message == null)
                    return new OperationResult { Success = false, Error = "Сообщение не найдено" };

                // Проверяем, является ли пользователь автором
                if (message.UserId == userId)
                {
                    await _unitOfWork.Messages.DeleteAsync(messageId);
                    await _unitOfWork.SaveAsync();

                    return new OperationResult { Success = true };
                }

                // Проверяем роли
                var userRoles = await _userManager.GetRolesAsync(new User { Id = userId });
                var hasPermission = userRoles.Any(r => r == "Admin" || r == "Mod");

                if (hasPermission)
                {
                    await _unitOfWork.Messages.DeleteAsync(messageId);
                    await _unitOfWork.SaveAsync();
                    return new OperationResult { Success = true };
                }

                return new OperationResult { Success = false, Error = "Недостаточно прав" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления сообщения {MessageId}", messageId);
                return new OperationResult { Success = false, Error = "Внутренняя ошибка сервера" };
            }
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
    }
}
