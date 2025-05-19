using CorpNetMessenger.Domain.DTOs;

namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IChatService
    {
        Task SaveMessage(ChatMessageDto request, string userId);
        Task<bool> EditMessage(string messageId, string newText);
    }
}
