using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace CorpNetMessenger.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly IChatService _chatService;
        private readonly IUnitOfWork _unitOfWork;
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _userGroups = new();
        private readonly IChatCacheService _chatCacheService;
        private readonly IMessageService _messageService;
        private readonly IUserContext _userContext;
        public ChatHub(ILogger<ChatHub> logger, IChatService chatService,
            IUnitOfWork unitOfWork, IChatCacheService chatCacheService,
            IMessageService messageService, IUserContext userContext)
        {
            _logger = logger;
            _chatService = chatService;
            _unitOfWork = unitOfWork;
            _chatCacheService = chatCacheService;
            _messageService = messageService;
            _userContext = userContext;
        }

        public async Task Enter(string chatId)
        {
            if (string.IsNullOrWhiteSpace(chatId))
            {
                await Clients.Caller.SendAsync("Error", "Неверный идентификатор чата");
                return;
            }

            string connectionId = Context.ConnectionId;
            var userId = _userContext.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                await Clients.Caller.SendAsync("Error", "Неавторизованный пользователь");
                return;
            }

            // Проверяем членство (перед добавлением в группу)
            if (!await _chatService.UserInChatAsync(chatId, userId))
            {
                _logger.LogInformation("Вход запрещен: user={UserId} connection={ConnectionId} chat={ChatId}", userId, connectionId, chatId);
                await Clients.Caller.SendAsync("Error", "Доступ запрещён");
                return;
            }

            // Добавляем в группу SignalR и в нашу карту групп для соединения
            await Groups.AddToGroupAsync(connectionId, chatId);
            var groups = _userGroups.GetOrAdd(connectionId, _ => new ConcurrentDictionary<string, byte>());
            groups.TryAdd(chatId, 0);
            _logger.LogInformation("Пользователь присоединился к чату: user={UserId} connection={ConnectionId} chat={ChatId}", userId, connectionId, chatId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string connectionId = Context.ConnectionId;
            if (_userGroups.TryRemove(connectionId, out var groups))
            {
                // удаляем из всех групп, где был
                var removeTasks = groups.Keys.Select(chatId => Groups.RemoveFromGroupAsync(connectionId, chatId));
                await Task.WhenAll(removeTasks);
                _logger.LogInformation("Соединение отключено и удалено из групп: connection={ConnectionId}", connectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task EditMessage(string messageId, string newText, string chatId)
        {
            var connectionId = Context.ConnectionId;
            var userId = _userContext.UserId;

            if (string.IsNullOrWhiteSpace(messageId) || string.IsNullOrWhiteSpace(chatId) || string.IsNullOrWhiteSpace(userId))
            {
                await Clients.Caller.SendAsync("Error", "Неверные параметры");
                return;
            }

            try
            {
                // Проверка принадлежности соощения чату
                var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
                if (message == null || message.ChatId != chatId)
                {
                    await Clients.Caller.SendAsync("Error", "Сообщение не найдено или не принадлежит чату");
                    return;
                }

                // Проверка членства в чате
                if (!await _chatService.UserInChatAsync(chatId, userId))
                {
                    await Clients.Caller.SendAsync("Error", "Доступ запрещён");
                    return;
                }

                var result = await _messageService.EditMessageAsync(messageId, newText, userId);
                if (result.Success)
                {
                    _chatCacheService.InvalidateChatCache(chatId);
                    await Clients.Group(chatId).SendAsync("UpdateMessage", messageId, newText);
                }
                else
                    await Clients.Caller.SendAsync("Error", result.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка редактирования сообщения user={UserId} connection={ConnectionId} message={MessageId}", userId, connectionId, messageId);
                await Clients.Caller.SendAsync("Error", "Внутренняя ошибка сервера");
            }
        }

        public async Task DeleteMessage(string messageId, string chatId)
        {
            var connectionId = Context.ConnectionId;
            var userId = _userContext.UserId;

            if (string.IsNullOrWhiteSpace(messageId) || string.IsNullOrWhiteSpace(chatId) || string.IsNullOrWhiteSpace(userId))
            {
                await Clients.Caller.SendAsync("Error", "Неверные параметры");
                return;
            }

            try
            {
                // Проверка принадлежности соощения чату
                var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
                if (message == null || message.ChatId != chatId)
                {
                    await Clients.Caller.SendAsync("Error", "Сообщение не найдено или не принадлежит чату");
                    return;
                }

                var result = await _messageService.DeleteMessageAsync(messageId, userId);
                if (result.Success)
                {
                    _chatCacheService.InvalidateChatCache(chatId);
                    await Clients.Group(chatId).SendAsync("RemoveMessage", messageId);
                }
                else
                    await Clients.Caller.SendAsync("Error", result.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка удаления сообщения {MessageId} user={UserId} connection={ConnectionId}", messageId, userId, connectionId);
                await Clients.Caller.SendAsync("Error", "Ошибка при удалении сообщения");
            }
        }

        public async Task LoadHistory(string chatId, int skip = 0, int take = 5)
        {
            var connectionId = Context.ConnectionId;
            var userId = _userContext.UserId;

            if (string.IsNullOrWhiteSpace(chatId) || string.IsNullOrWhiteSpace(userId))
            {
                await Clients.Caller.SendAsync("Error", "Неверные параметры");
                return;
            }

            try
            {
                // Проверка членства в чате
                if (!await _chatService.UserInChatAsync(chatId, userId))
                {
                    await Clients.Caller.SendAsync("Error", "Доступ запрещён");
                    return;
                }

                // Ограничиваем размер выборки
                take = Math.Clamp(take, 1, 50);

                var messages = await _messageService.LoadHistoryChatAsync(chatId, skip, take);
                await Clients.Caller.SendAsync("ReceiveHistory", messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки истории для чата {ChatId} user={UserId} connection={ConnectionId}", chatId, userId, connectionId);
                await Clients.Caller.SendAsync("Error", "Ошибка загрузки истории");
            }
        }
    }
}
