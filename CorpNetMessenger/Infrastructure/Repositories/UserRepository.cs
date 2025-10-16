using CorpNetMessenger.Application.Configs;
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

        public async Task<IReadOnlyCollection<ContactViewModel>> GetAllDepartmentContactsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context
                .Users.Where(u => u.DepartmentId == id && u.StatusId == StatusTypes.Active)
                .Include(u => u.Post)
                .Select(u => new ContactViewModel
                {
                    Id = u.Id,
                    UserName = string.Format("{0} {1}", u.LastName, u.Name),
                    PostName = u.Post.Title,
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<ContactViewModel>> GetAllDepartmentContactsAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null)
                throw new Exception("Пользователь не найден");

            var departmentId = user.DepartmentId;
            if (departmentId == null)
                throw new Exception("Пользователь не принадлежит ни к одному отделу!");

            return await GetAllDepartmentContactsAsync(departmentId.Value, cancellationToken);
        }

        public async Task<IEnumerable<User>> GetAllNewUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.Post)
                .Include(u => u.Department)
                .Where(u => u.StatusId == 1)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<User>> GetAllUserWithDetailsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.Post)
                .Include(u => u.Department)
                .Include(u => u.Status)
                .ToListAsync(cancellationToken);
        }

        public async Task<User> GetByIdWithDetailsAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Post)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<List<User>> SearchContactsByNameAsync(string name, int departmentId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.Post)
                .Where(u => (u.Name.Contains(name) || u.LastName.Contains(name) || u.Patronymic.Contains(name) || u.Post.Title.Contains(name)) && u.DepartmentId == departmentId)
                .ToListAsync(cancellationToken);
        }
    }
}
