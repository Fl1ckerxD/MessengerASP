using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Infrastructure.Data;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    internal class AttachmentRepository : Repository<Domain.Entities.Attachment>
    {
        public AttachmentRepository(MessengerContext context) : base(context)
        {
        }
        public override async Task<Attachment?> GetByIdAsync(string id)
        {
            var guid = Guid.Parse(id);
            return await _context.Attachments.FindAsync(guid);
        }
    }
}