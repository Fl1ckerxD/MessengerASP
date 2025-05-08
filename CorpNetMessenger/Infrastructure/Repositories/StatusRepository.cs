using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Infrastructure.Data;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    internal class StatusRepository : Repository<Status>
    {
        public StatusRepository(MessengerContext context) : base(context)
        {
        }
    }
}