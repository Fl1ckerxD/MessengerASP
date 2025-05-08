using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Infrastructure.Data;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    internal class UserRepository : Repository<User>
    {
        public UserRepository(MessengerContext context) : base(context)
        {
        }
    }
}