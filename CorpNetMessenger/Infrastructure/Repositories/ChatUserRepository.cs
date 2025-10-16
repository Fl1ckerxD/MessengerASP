using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    public class ChatUserRepository : Repository<ChatUser>, IChatUserRepository
    {
        public ChatUserRepository(MessengerContext context) : base(context)
        {
        }

        public async Task<ChatUser?> GetByPredicateAsync(Expression<Func<ChatUser, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.ChatUsers.FirstOrDefaultAsync(predicate, cancellationToken);
        }
    }
}
