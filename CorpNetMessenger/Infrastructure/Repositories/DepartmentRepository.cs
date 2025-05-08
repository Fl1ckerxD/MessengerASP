using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Infrastructure.Data;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    internal class DepartmentRepository : Repository<Department>
    {
        public DepartmentRepository(MessengerContext context) : base(context)
        {
        }
    }
}