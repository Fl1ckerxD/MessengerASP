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
        public ChatHub(ILogger<ChatHub> logger, IChatService chatService)
        {
            _logger = logger;
            _chatService = chatService;
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
