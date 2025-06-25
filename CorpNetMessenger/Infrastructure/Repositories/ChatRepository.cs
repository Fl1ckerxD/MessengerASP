using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    internal class ChatRepository : Repository<Chat>, IChatRepository
    {
        public ChatRepository(MessengerContext context) : base(context)
        {
        }

        public async Task<Chat?> GetByDepartmentIdAsync(int id)
        {
            return await _context.Chats.FirstOrDefaultAsync(c => c.DepartmentId == id);
        }
    }
}