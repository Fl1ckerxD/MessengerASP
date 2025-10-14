using CorpNetMessenger.Domain.Entities;

namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IChatService
    {
        Task<bool> UserInChatAsync(string chatId, string userId, CancellationToken cancellationToken = default);
        Task<Chat> GetDepartmentChatForUserAsync(string userId, CancellationToken cancellationToken = default);
        Task AddUserToChatAsync(string userId, string chatId, CancellationToken cancellationToken = default);
        Task AddUserToChatAsync(User user, string chatId, CancellationToken cancellationToken = default);
        Task AddUserToDepartmentChatAsync(User user, CancellationToken cancellationToken = default);
    }
}
