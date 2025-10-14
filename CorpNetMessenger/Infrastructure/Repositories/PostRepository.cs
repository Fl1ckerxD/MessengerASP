using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    internal class PostRepository : Repository<Post>, IPostRepository
    {
        public PostRepository(MessengerContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DepartmentPost>> GetByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken = default)
        {
            return await _context.DepartmentPost
                .Include(dp => dp.Post)
                .Where(dp => dp.DepartmentId == departmentId)
                .ToListAsync(cancellationToken);
        }
    }
}