using CorpNetMessenger.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CorpNetMessenger.Web.Views.Hubs
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
            var user = Context.User;
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }

        public async Task EditMessage(string messageId, string newText, string chatId)
        {
            var isEdited = await _chatService.EditMessage(messageId, newText);
            if (isEdited)
                Clients.Group(chatId).SendAsync("UpdateMessage", messageId, newText);
        }
    }
}
