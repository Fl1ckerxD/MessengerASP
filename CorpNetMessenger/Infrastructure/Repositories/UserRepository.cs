using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Infrastructure.Data;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    internal class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(MessengerContext context)
            : base(context) { }

        public async Task<List<ContactViewModel>> GetAllDepartmentContactsAsync(int id)
        {
            return await _context
                .Users.Where(u => u.DepartmentId == id)
                .Include(u => u.Post)
                .Select(u => new ContactViewModel
                {
                    Id = u.Id,
                    UserName = string.Format("{0} {1}", u.LastName, u.Name),
                    PostName = u.Post.Title,
                })
                .ToListAsync();
        }

        public async Task<List<ContactViewModel>> GetAllDepartmentContactsAsync(string userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                throw new Exception("Пользователь не найден");

            var departmentId = user.DepartmentId;
            if (departmentId == null)
                throw new Exception("Пользователь не принадлежит ни к одному отделу!");

            return await GetAllDepartmentContactsAsync(departmentId.Value);
        }

        public async Task<List<User>> SearchContactsByNameAsync(string name, int departmentId)
        {
            return await _context.Users
            .Include(u => u.Post)
            .Where(u => (u.Name.Contains(name) || u.LastName.Contains(name) || u.Patronymic.Contains(name) || u.Post.Title.Contains(name)) && u.DepartmentId == departmentId)
            .ToListAsync();
        }
    }
}
