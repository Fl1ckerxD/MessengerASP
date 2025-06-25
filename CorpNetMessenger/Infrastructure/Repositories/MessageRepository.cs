using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    internal class MessageRepository : Repository<Message>, IMessageRepository
    {
        public MessageRepository(MessengerContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Message>> LoadHistoryChatAsync(string chatId, int skip = 0, int take = 5)
        {
            return await _context.Messages // Получение сообщений из определенного чата 
                .Where(m => m.ChatId == chatId)
                .Include(m => m.User)
                .Include(m => m.Attachments)
                .OrderByDescending(m => m.SentAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<Message> GetMessageWithDetailsAsync(string id)
        {
            return await _context.Messages
                .Include(m => m.User)
                .Include(m => m.Attachments)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}