using CorpNetMessenger.Domain.Entities;

namespace CorpNetMessenger.Domain.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IChatRepository Chats { get; }
        IRepository<Department> Departments { get; }
        IRepository<Attachment> Files { get; }
        IMessageRepository Messages { get; }
        IRepository<Post> Posts { get; }
        IRepository<Status> Statuses { get; }
        IUserRepository Users { get; }
        IChatUserRepository ChatUsers { get; }
        Task<int> SaveAsync();
    }
}
