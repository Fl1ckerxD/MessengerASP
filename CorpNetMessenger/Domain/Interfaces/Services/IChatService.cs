using CorpNetMessenger.Application.Common;
using CorpNetMessenger.Domain.DTOs;

namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IChatService
    {
        Task<string> SaveMessage(ChatMessageDto request, string userId);
        Task<OperationResult> EditMessage(string messageId, string newText, string userId);
        Task<OperationResult> DeleteMessage(string messageId, string userId);
        Task<bool> UserInChat(string chatId, string userId);
        Task<MessageDto> GetMessageAsync(string messageId);
        Task<IEnumerable<MessageDto>> LoadHistoryChatAsync(string chatId, int skip = 0, int take = 5);
    }
}
