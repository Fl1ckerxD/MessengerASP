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
        private static readonly ConcurrentDictionary<string, string> UserGroups = new();
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
            string connectionId = Context.ConnectionId;

            // Проверка уже существующего подключения
            if (UserGroups.TryGetValue(connectionId, out var currentChat))
            {
                if (currentChat == chatId) return; // Уже в этом чате
                await Groups.RemoveFromGroupAsync(connectionId, currentChat);
            }

            bool isInChat = await _chatService.UserInChatAsync(chatId, _userContext.UserId);
            if (!isInChat)
            {
                await Clients.Caller.SendAsync("Error", "Доступ запрещён");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
            UserGroups[connectionId] = chatId;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string connectionId = Context.ConnectionId;
            if (UserGroups.TryRemove(connectionId, out var chatId))
            {
                await Groups.RemoveFromGroupAsync(connectionId, chatId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task EditMessage(string messageId, string newText, string chatId)
        {
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
                if (!await _chatService.UserInChatAsync(chatId, _userContext.UserId))
                {
                    await Clients.Caller.SendAsync("Error", "Доступ запрещён");
                    return;
                }

                var result = await _messageService.EditMessageAsync(messageId, newText, _userContext.UserId);

                if (result.Success)
                {
                    _chatCacheService.InvalidateChatCache(chatId);
                    Clients.Group(chatId).SendAsync("UpdateMessage", messageId, newText);
                }
                else
                    await Clients.Caller.SendAsync("Error", result.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка редактирования сообщения");
                await Clients.Caller.SendAsync("Error", "Внутренняя ошибка сервера");
            }
        }

        public async Task DeleteMessage(string messageId, string chatId)
        {
            try
            {
                // Проверка принадлежности соощения чату
                var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
                if (message == null || message.ChatId != chatId)
                {
                    await Clients.Caller.SendAsync("Error", "Сообщение не найдено или не принадлежит чату");
                    return;
                }

                var result = await _messageService.DeleteMessageAsync(messageId, _userContext.UserId);

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
                _logger.LogError(ex, "Ошибка удаления сообщения {MessageId}", messageId);
                await Clients.Caller.SendAsync("Error", "Ошибка при удалении сообщения");
            }
        }

        public async Task LoadHistory(string chatId, int skip = 0, int take = 5)
        {
            try
            {
                // Проверка членства в чате
                if (!await _chatService.UserInChatAsync(chatId, _userContext.UserId))
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
                _logger.LogError(ex, "Ошибка загрузки истории для чата {ChatId}", chatId);
                await Clients.Caller.SendAsync("Error", "Ошибка загрузки истории");
            }
        }
    }
}
