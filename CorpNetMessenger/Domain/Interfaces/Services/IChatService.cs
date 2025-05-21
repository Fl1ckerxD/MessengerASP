using CorpNetMessenger.Application.Common;
using CorpNetMessenger.Domain.DTOs;

namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IChatService
    {
        Task SaveMessage(ChatMessageDto request, string userId);
        Task<OperationResult> EditMessage(string messageId, string newText, string userId);
        Task<bool> UserInChat(string chatId, string userId);
    }
}
