using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Infrastructure.Data;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    internal class MessageRepository : Repository<Message>, IMessageRepository
    {
        public MessageRepository(MessengerContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MessageViewModel>> LoadHistoryChatAsync(string chatId, int skip = 0, int take = 5)
        {
            if (!_context.Chats.Any(c => c.Id == chatId)) // Проверка на наличие чата с таким id
                throw new Exception("Такого чата нет");
            
            return await _context.Messages // Получение сообщений из определенного чата 
                .Where(m => m.ChatId == chatId)
                .Include(m => m.User)
                .Include(m => m.Attachments)
                .OrderByDescending(m => m.SentAt)
                .Skip(skip)
                .Take(take)
                .Select(m => new MessageViewModel
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    Content = m.Content,
                    SentAt = m.SentAt,
                    UserName = $"{m.User.LastName} {m.User.Name}",
                    Attachments = m.Attachments.Select(c => new AttachmentViewModel
                    {
                        Id = c.Id.ToString(),
                        Name = c.FileName
                    })
                }).ToListAsync();
        }
    }
}