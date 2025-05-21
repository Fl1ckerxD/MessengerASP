using CorpNetMessenger.Domain.Entities;
using System.Linq.Expressions;

namespace CorpNetMessenger.Domain.Interfaces.Repositories
{
    public interface IChatUserRepository
    {
        Task<ChatUser?> GetByPredicateAsync(Expression<Func<ChatUser, bool>> predicate);
    }
}
