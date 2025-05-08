using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Infrastructure.Data;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    internal class MessageRepository : Repository<Message>
    {
        public MessageRepository(MessengerContext context) : base(context)
        { 
        }
    }
}