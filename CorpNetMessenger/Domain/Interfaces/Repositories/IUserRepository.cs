using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;

namespace CorpNetMessenger.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<List<ContactViewModel>> GetAllDepartmentContactsAsync(int departmentId);
        Task<List<ContactViewModel>> GetAllDepartmentContactsAsync(string userId);
        Task<List<User>> SearchContactsByNameAsync(string name, int departmentId);
    }
}
