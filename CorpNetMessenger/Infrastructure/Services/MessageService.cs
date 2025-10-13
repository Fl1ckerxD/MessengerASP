using AutoMapper;
using CorpNetMessenger.Application.Common;
using CorpNetMessenger.Application.Configs;
using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace CorpNetMessenger.Infrastructure.Services
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MessageService> _logger;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly IChatCacheService _chatCacheService;
        private readonly UserManager<User> _userManager;
        private readonly IChatService _chatService;
        private readonly IMessageQueue<Message> _messageQueue;

        private const string RoleAdmin = "Admin";
        private const string RoleMod = "Mod";
        private const string CacheKeyPattern = "chat_history_{0}_skip{1}_take{2}";

        public MessageService(IUnitOfWork unitOfWork, ILogger<MessageService> logger,
            IMapper mapper, UserManager<User> userManager,
            IMemoryCache cache, IChatCacheService chatCacheService,
            IChatService chatService, IMessageQueue<Message> messageQueue)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _cache = cache;
            _chatCacheService = chatCacheService;
            _chatService = chatService;
            _messageQueue = messageQueue;
        }

        /// <summary>
        /// Сохранение сообщения в БД
        /// </summary>
        /// <param name="content">Текст сообщения</param>
        /// <param name="userId">Id пользователя отправившего сообщение</param>
        /// <param name="chatId">Чат в который было отправлено сообщение</param>
        /// <returns>Возвращает Id сохраненного сообщения</returns>
        public async Task<MessageDto> SaveMessageAsync(ChatMessageDto request, string userId)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(userId, nameof(userId));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(request.ChatId, nameof(request.ChatId));

            try
            {
                bool isUserInChat = await _chatService.UserInChatAsync(request.ChatId, userId);

                if (!isUserInChat)
                    throw new UnauthorizedAccessException("Пользователь не состоит в чате");

                var text = (request.Text ?? string.Empty).Trim();
                if (text.Length > MessagingOptions.MaxMessageLength)
                    throw new ArgumentException($"Длина сообщения превышает допустимый лимит ({MessagingOptions.MaxMessageLength})", nameof(request.Text));

                if (request.Files != null && request.Files.Count > MessagingOptions.MaxFileCount)
                    throw new ArgumentException($"Количество вложений превышает допустимый лимит ({MessagingOptions.MaxFileCount})", nameof(request.Files));

                // Создаём сущность сообщения и присваиваем Id заранее
                var message = new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    ChatId = request.ChatId,
                    Content = text,
                    UserId = userId,
                    Attachments = request.Files
                };

                // Помещаем в очередь для фоновой записи
                var enqueued = await _messageQueue.EnqueueAsync(message);
                if (!enqueued)
                {
                    _logger.LogWarning("Очередь переполнена, не удалось поставить сообщение в очередь: user={UserId} chat={ChatId}", userId, request.ChatId);
                    throw new InvalidOperationException("Сервис временно недоступен, попробуйте позже");
                }

                var messageDto = _mapper.Map<MessageDto>(message);

                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var userDto = _mapper.Map<UserDto>(user);
                    messageDto.User = userDto;
                }

                // Возвращаем dto, запись в БД произойдёт фоновой задачей
                return messageDto;
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogInformation("Пользователь {UserId} попытался отправить сообщение в чат {ChatId}", userId, request?.ChatId);
                throw;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Операция сохранения сообщения отменена для пользователя {UserId}", userId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении сообщения в чате {ChatId} пользователем {UserId}", request?.ChatId, userId);
                throw;
            }
        }

        /// <summary>
        /// Редактирование сообщения
        /// </summary>
        /// <param name="messageId">Id конкретного сообщения для редактирования</param>
        /// <param name="newText">Отредактированный текст</param>
        /// <returns>Возвращает true если сообщение было отредактировано иначе false</returns>
        public async Task<OperationResult> EditMessageAsync(string messageId, string newText, string userId)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);

            if (message == null)
                return new OperationResult { Success = false, Error = "Сообщение не найдено" };

            if (message.UserId != userId)
                return new OperationResult { Success = false, Error = "Нельзя редактировать чужое сообщение" };

            if (string.IsNullOrWhiteSpace(newText))
                return new OperationResult { Success = false, Error = "Текст не может быть пустым" };

            if (newText.Length > MessagingOptions.MaxMessageLength)
                return new OperationResult { Success = false, Error = "Сообщение слишком длинное" };

            message.Content = newText;
            await _unitOfWork.SaveAsync();

            return new OperationResult { Success = true };
        }

        /// <summary>
        /// Получает сообщение по его идентификатору и преобразует в DTO для передачи клиенту.
        /// </summary>
        /// <param name="messageId">Идентификатор сообщения</param>
        /// <returns>Объект <see cref="MessageDto"/>, представляющий сообщение с дополнительными данными</returns>
        /// <exception cref="ArgumentNullException">Если сообщение с указанным ID не найдено</exception>
        public async Task<MessageDto> GetMessageAsync(string messageId)
        {
            try
            {
                var messageEntity = await _unitOfWork.Messages.GetMessageWithDetailsAsync(messageId);
                if (messageEntity == null)
                    throw new ArgumentNullException(nameof(messageId), "Сообщение не найдено");

                return _mapper.Map<MessageDto>(messageEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении сообщения {MessageId}", messageId);
                throw;
            }
        }

        /// <summary>
        /// Загружает историю сообщений чата с пагинацией
        /// </summary>
        /// <param name="chatId">Идентификатор чата</param>
        /// <param name="skip">Количество пропускаемых сообщений</param>
        /// <param name="take">Количество загружаемых сообщений (1-100)</param>
        /// <returns>Коллекцию сообщений в формате DTO</returns>
        public async Task<IEnumerable<MessageDto>> LoadHistoryChatAsync(string chatId, int skip = 0, int take = 5)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(chatId, nameof(chatId));
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));
            if (take < 1 || take > 100) throw new ArgumentOutOfRangeException(nameof(take));

            // Проверка существования чата
            if (!await _unitOfWork.Chats.AnyAsync(c => c.Id == chatId))
                throw new KeyNotFoundException("Чат не найден");

            var cacheKey = string.Format(CacheKeyPattern, chatId, skip, take);
            _chatCacheService.RegisterCacheKey(chatId, cacheKey);

            if (_cache.TryGetValue(cacheKey, out List<MessageDto> cachedMessages))
                return cachedMessages;

            var messages = await _unitOfWork.Messages.LoadHistoryChatAsync(chatId, skip, take);
            var messageDtos = _mapper.Map<List<MessageDto>>(messages);

            // Сохранение в кэш
            _cache.Set(cacheKey, messageDtos, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });

            return messageDtos;
        }

        /// <summary>
        /// Удаляет сообщение с проверкой прав пользователя
        /// </summary>
        /// <param name="messageId">Идентификатор сообщения</param>
        /// <param name="userId">Идентификатор пользователя, инициирующего удаление</param>
        /// <returns>Результат операции</returns>
        public async Task<OperationResult> DeleteMessageAsync(string messageId, string userId)
        {
            try
            {
                // Поиск сообщения
                var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
                if (message == null)
                    return new OperationResult { Success = false, Error = "Сообщение не найдено" };

                // Проверка прав: автор может удалить свое сообщение
                if (message.UserId == userId)
                {
                    await _unitOfWork.Messages.DeleteAsync(messageId);
                    await _unitOfWork.SaveAsync();
                    return new OperationResult { Success = true };
                }

                // Получаем реального пользователя для проверки ролей
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return new OperationResult { Success = false, Error = "Пользователь не найден" };

                // Проверка прав: админ или модератор может удалить любое сообщение
                var userRoles = await _userManager.GetRolesAsync(user);
                var hasPermission = userRoles.Any(r => r == RoleAdmin || r == RoleMod);

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
    }
}