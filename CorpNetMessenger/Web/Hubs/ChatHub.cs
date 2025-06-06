using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CorpNetMessenger.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly IChatService _chatService;
        private readonly IUnitOfWork _unitOfWork;
        public ChatHub(ILogger<ChatHub> logger, IChatService chatService,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _chatService = chatService;
            _unitOfWork = unitOfWork;
        }

        public async Task Enter(string chatId)
        {
            string userId = GetUserId();

            bool isInChat = await _chatService.UserInChat(chatId, userId);
            if (!isInChat)
            {
                await Clients.Caller.SendAsync("Error", "Доступ запрещён");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }

        public async Task EditMessage(string messageId, string newText, string chatId)
        {
            try
            {
                string userId = GetUserId();

                var result = await _chatService.EditMessage(messageId, newText, userId);

                if (result.Success)
                    Clients.Group(chatId).SendAsync("UpdateMessage", messageId, newText);
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
            var user = Context.User;
            string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
            if (message == null)
                return;

            if (message.UserId != userId)
                return; // Проверка прав

            await _unitOfWork.Messages.DeleteAsync(messageId);
            await _unitOfWork.SaveAsync();

            await Clients.Group(chatId).SendAsync("RemoveMessage", messageId);
        }

        public async Task LoadHistory(string chatId, int skip = 0, int take = 5)
        {
            var messages = await _unitOfWork.Messages.LoadHistoryChatAsync(chatId, skip, take);
            await Clients.Caller.SendAsync("ReceiveHistory", messages);
        }

        private string GetUserId()
        {
            var user = Context.User;
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
