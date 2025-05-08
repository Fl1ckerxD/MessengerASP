using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Infrastructure.Data;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    internal class ChatRepository : Repository<Chat>
    {
        public ChatRepository(MessengerContext context) : base(context)
        {
        }
    }
}