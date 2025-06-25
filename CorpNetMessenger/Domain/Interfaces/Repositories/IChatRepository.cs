using CorpNetMessenger.Domain.Entities;

namespace CorpNetMessenger.Domain.Interfaces.Repositories
{
    public interface IChatRepository : IRepository<Chat>
    {
        Task<Chat?> GetByDepartmentIdAsync(int id);
    }
}
