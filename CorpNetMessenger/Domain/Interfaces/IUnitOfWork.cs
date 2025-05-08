using CorpNetMessenger.Domain.Entities;

namespace CorpNetMessenger.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Chat> Chats { get; }
        IRepository<Department> Departments { get; }
        IRepository<Entities.File> Files { get; }
        IRepository<Message> Messages { get; }
        IRepository<Post> Posts { get; }
        IRepository<Status> Statuses { get; }
        IRepository<User> Users { get; }
        Task<int> SaveAsync();
    }
}
