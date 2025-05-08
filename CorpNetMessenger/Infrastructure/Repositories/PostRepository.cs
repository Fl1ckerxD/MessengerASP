using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Infrastructure.Data;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    internal class PostRepository : Repository<Post>
    {
        public PostRepository(MessengerContext context) : base(context)
        {
        }
    }
}