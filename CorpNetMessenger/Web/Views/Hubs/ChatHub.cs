using CorpNetMessenger.Domain.DTOs;
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
        //private readonly IChatService _chatService;
        public ChatHub(ILogger<ChatHub> logger, IChatService chatService)
        {
            _logger = logger;
            //_chatService = chatService;
        }

        public async Task Enter(string chatId)
        {
            var user = Context.User;
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }

        //public async Task Send(string message, List<AttachmentDto> files, string chatId)
        //{
        //    var user = Context.User;
        //    string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        //    try
        //    {
        //        //await _chatService.SaveMessage(message, userId, chatId);
        //        await Clients.Group(chatId).SendAsync("Receive", message, user.Identity.Name, DateTime.Now);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Ошибка сохранения сообщения в бд");
        //    }
        //}
    }
}
