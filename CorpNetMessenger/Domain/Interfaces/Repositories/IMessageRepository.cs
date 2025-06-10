using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;

namespace CorpNetMessenger.Domain.Interfaces.Repositories
{
    public interface IMessageRepository : IRepository<Message>
    {
        Task<Message> GetMessageWithDetailsAsync(string id);
        Task<IEnumerable<MessageViewModel>> LoadHistoryChatAsync(string chatId, int skip = 0, int take = 5);
    }
}
