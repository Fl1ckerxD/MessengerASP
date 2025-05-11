using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Infrastructure.Data;

namespace CorpNetMessenger.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MessengerContext _context;
        private bool _disposed;
        public IRepository<Chat> _chats;
        public IRepository<Department> _departments;
        public IRepository<Domain.Entities.File> _files;
        public IMessageRepository _messages;
        public IRepository<Post> _posts;
        public IRepository<Status> _statuses;
        public IRepository<User> _users;

        public UnitOfWork(MessengerContext context)
        {
            _context = context;
        }

        public IRepository<Chat> Chats => _chats ??= new ChatRepository(_context);
        public IRepository<Department> Departments => _departments ??= new DepartmentRepository(_context);
        public IRepository<Domain.Entities.File> Files => _files ??= new FileRepository(_context);
        public IMessageRepository Messages => _messages ??= new MessageRepository(_context);
        public IRepository<Post> Posts => _posts ??= new PostRepository(_context);
        public IRepository<Status> Statuses => _statuses ??= new StatusRepository(_context);
        public IRepository<User> Users => _users ??= new UserRepository(_context);

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
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
