using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;

namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IEmployeeService
    {
        Task<EmployeeDto> GetEmployeeInfo(string id);
        Task<IEnumerable<ContactViewModel>> SearchEmployees(string term, int departmentId, string userId);
    }
}