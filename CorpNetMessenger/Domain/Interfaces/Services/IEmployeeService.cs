using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;

namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IEmployeeService
    {
        Task<EmployeeDto> GetEmployeeInfoAsync(string id);
        Task<IEnumerable<ContactViewModel>> SearchEmployeesAsync(string term, int departmentId, string userId);
    }
}