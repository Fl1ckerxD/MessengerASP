using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Infrastructure.Data;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    internal class AttachmentRepository : Repository<Attachment>
    {
        public AttachmentRepository(MessengerContext context) : base(context)
        {
        }
        public override async Task<Attachment?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            var guid = Guid.Parse(id);
            return await _context.Attachments.FindAsync(guid, cancellationToken);
        }
    }
}