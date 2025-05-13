using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Web.ViewModels;

namespace CorpNetMessenger.Domain.Interfaces.Repositories
{
    public interface IMessageRepository : IRepository<Message>
    {
        Task<IEnumerable<MessageViewModel>> GetChatMessagesAsync(string chatId);
    }
}
