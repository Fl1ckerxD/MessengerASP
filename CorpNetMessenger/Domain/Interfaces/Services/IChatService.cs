using CorpNetMessenger.Domain.Entities;

namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IChatService
    {
        Task<bool> UserInChatAsync(string chatId, string userId);
        Task<Chat> GetDepartmentChatForUserAsync(string userId);
        Task AddUserToChatAsync(string userId, string chatId);
        Task AddUserToChatAsync(User user, string chatId);
        Task AddUserToDepartmentChatAsync(User user);
    }
}
