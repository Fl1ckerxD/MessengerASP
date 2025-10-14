using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;

namespace CorpNetMessenger.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<IReadOnlyCollection<ContactViewModel>> GetAllDepartmentContactsAsync(int departmentId, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<ContactViewModel>> GetAllDepartmentContactsAsync(string userId, CancellationToken cancellationToken = default);
        Task<List<User>> SearchContactsByNameAsync(string name, int departmentId, CancellationToken cancellationToken = default);
        Task<User> GetByIdWithDetailsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetAllNewUsersAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetAllUserWithDetailsAsync(CancellationToken cancellationToken = default);
    }
}
