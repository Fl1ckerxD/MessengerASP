using CorpNetMessenger.Application.Common;
using CorpNetMessenger.Domain.DTOs;

namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IMessageService
    {
        Task<MessageDto> SaveMessageAsync(ChatMessageDto request, string userId, CancellationToken cancellationToken = default);
        Task<OperationResult> EditMessageAsync(string messageId, string newText, string userId, CancellationToken cancellationToken = default);
        Task<MessageDto> GetMessageAsync(string messageId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MessageDto>> LoadHistoryChatAsync(string chatId, int skip = 0, int take = 5, CancellationToken cancellationToken = default);
        Task<OperationResult> DeleteMessageAsync(string messageId, string userId, CancellationToken cancellationToken = default);
    }
}