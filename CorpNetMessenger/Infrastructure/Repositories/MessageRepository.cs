using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Infrastructure.Data;
using CorpNetMessenger.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    internal class MessageRepository : Repository<Message>, IMessageRepository
    {
        public MessageRepository(MessengerContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MessageViewModel>> GetChatMessagesAsync()
        {
            return await _context.Messages.Include(m => m.User)
                .Select(m => new MessageViewModel
                {
                    Content = m.Content,
                    SentAt = m.SentAt,
                    UserName = $"{m.User.LastName} {m.User.Name}"
                }).OrderBy(m => m.SentAt).ToListAsync();
        }
    }
}