﻿using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;

namespace CorpNetMessenger.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<IReadOnlyCollection<ContactViewModel>> GetAllDepartmentContactsAsync(int departmentId);
        Task<IReadOnlyCollection<ContactViewModel>> GetAllDepartmentContactsAsync(string userId);
        Task<List<User>> SearchContactsByNameAsync(string name, int departmentId);
        Task<User> GetByIdWithDetailsAsync(string id);
        Task<IEnumerable<User>> GetAllNewUsersAsync();
        Task<IEnumerable<User>> GetAllUserWithDetailsAsync();
    }
}
