using CorpNetMessenger.Domain.Entities;

namespace CorpNetMessenger.Domain.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Chat> Chats { get; }
        IRepository<Department> Departments { get; }
        IRepository<Entities.Attachment> Files { get; }
        IMessageRepository Messages { get; }
        IRepository<Post> Posts { get; }
        IRepository<Status> Statuses { get; }
        IRepository<User> Users { get; }
        Task<int> SaveAsync();
    }
}
