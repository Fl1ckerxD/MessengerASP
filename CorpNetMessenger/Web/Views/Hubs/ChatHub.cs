using CorpNetMessenger.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CorpNetMessenger.Web.Views.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        public async Task Enter(string chatId)
        {
            var user = Context.User;
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }

        public async Task Send(string message, string chatId, IChatService chatService)
        {
            var user = Context.User;
            string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await chatService.SaveMessage(message, userId, chatId);
                await Clients.Group(chatId).SendAsync("Receive", message, user.Identity.Name, DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка сохранения сообщения в бд");
            }
        }
    }
}
