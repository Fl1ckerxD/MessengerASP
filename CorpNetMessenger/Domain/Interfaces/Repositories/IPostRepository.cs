using CorpNetMessenger.Domain.Entities;

namespace CorpNetMessenger.Domain.Interfaces.Repositories
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<IEnumerable<DepartmentPost>> GetByDepartmentIdAsync(int departmentId);
    }
}