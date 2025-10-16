using CorpNetMessenger.Domain.Entities;

namespace CorpNetMessenger.Domain.Interfaces.Repositories
{
    public interface IMessageRepository : IRepository<Message>
    {
        Task<Message> GetMessageWithDetailsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Message>> LoadHistoryChatAsync(string chatId, int skip = 0, int take = 5, CancellationToken cancellationToken = default);
    }
}
