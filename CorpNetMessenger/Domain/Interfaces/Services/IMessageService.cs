using CorpNetMessenger.Application.Common;
using CorpNetMessenger.Domain.DTOs;

namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IMessageService
    {
        Task<string> SaveMessage(ChatMessageDto request, string userId);
        Task<OperationResult> EditMessage(string messageId, string newText, string userId);
        Task<MessageDto> GetMessageAsync(string messageId);
        Task<IEnumerable<MessageDto>> LoadHistoryChatAsync(string chatId, int skip = 0, int take = 5);
        Task<OperationResult> DeleteMessage(string messageId, string userId);
    }
}