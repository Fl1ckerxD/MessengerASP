﻿using AutoMapper;
using CorpNetMessenger.Application.Common;
using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using System.Collections;
using System.Reflection;
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
        private readonly IChatCacheService _chatCacheService;
        public ChatService(IUnitOfWork unitOfWork, ILogger<ChatService> logger,
            IMapper mapper, UserManager<User> userManager, IMemoryCache cache, IChatCacheService chatCacheService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _cache = cache;
            _chatCacheService = chatCacheService;
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

        /// <summary>
        /// Загружает историю сообщений чата с пагинацией
        /// </summary>
        /// <param name="chatId">Идентификатор чата</param>
        /// <param name="skip">Количество пропускаемых сообщений</param>
        /// <param name="take">Количество загружаемых сообщений (1-100)</param>
        /// <returns>Коллекцию сообщений в формате DTO</returns>
        public async Task<IEnumerable<MessageDto>> LoadHistoryChatAsync(string chatId, int skip = 0, int take = 5)
        {
            if (string.IsNullOrWhiteSpace(chatId))
                throw new ArgumentException("Chat ID не может быть пустым", nameof(chatId));

            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));
            if (take < 1 || take > 100) throw new ArgumentOutOfRangeException(nameof(take));

            var cacheKey = $"chat_history_{chatId}_skip{skip}_take{take}";
            _chatCacheService.RegisterCacheKey(chatId, cacheKey);
            if (_cache.TryGetValue(cacheKey, out List<MessageDto> cachedMessages))
            {
                return cachedMessages;
            }

            // Проверка существования чата
            if (!await _unitOfWork.Chats.AnyAsync(c => c.Id == chatId))
                throw new Exception("Такого чата нет");

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
        public async Task<OperationResult> DeleteMessage(string messageId, string userId)
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

                // Проверка прав: админ или модератор может удалить любое сообщение
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
        /// Добавляет пользователя в чат
        /// </summary>
        /// <param name="user">Объект пользователя</param>
        /// <param name="chat">Объект чата</param>
        public async Task AddUserToChat(User user, Chat chat)
        {
            ArgumentNullException.ThrowIfNull(chat);
            await AddUserToChat(user, chat.Id);
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
            var chat = await _unitOfWork.Chats.GetByDepartmentIdAsync(user.DepartmentId.Value);
            await AddUserToChat(user, chat);
        }
    }
}
