using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Infrastructure.Data;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MessengerContext _context;
        private bool _disposed;
        private IChatRepository _chats;
        private IRepository<Department> _departments;
        private IRepository<Attachment> _files;
        private IMessageRepository _messages;
        private IPostRepository _posts;
        private IRepository<Status> _statuses;
        private IUserRepository _users;
        private IChatUserRepository _chatUsers;

        public UnitOfWork(MessengerContext context)
        {
            _context = context;
        }

        public IChatRepository Chats => _chats ??= new ChatRepository(_context);
        public IRepository<Department> Departments => _departments ??= new DepartmentRepository(_context);
        public IRepository<Attachment> Files => _files ??= new AttachmentRepository(_context);
        public IMessageRepository Messages => _messages ??= new MessageRepository(_context);
        public IPostRepository Posts => _posts ??= new PostRepository(_context);
        public IRepository<Status> Statuses => _statuses ??= new StatusRepository(_context);
        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IChatUserRepository ChatUsers => _chatUsers ??= new ChatUserRepository(_context);

        public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
