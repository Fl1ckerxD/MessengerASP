using CorpNetMessenger.Infrastructure.Data;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    internal class FileRepository : Repository<Domain.Entities.File>
    {
        public FileRepository(MessengerContext context) : base(context)
        {
        }
    }
}