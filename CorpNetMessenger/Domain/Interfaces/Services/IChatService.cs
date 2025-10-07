using CorpNetMessenger.Domain.Entities;

namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IChatService
    {
        Task<bool> UserInChat(string chatId, string userId);
        Task<Chat> GetDepartmentChatForUserAsync(string userId);
        Task AddUserToChat(string userId, string chatId);
        Task AddUserToChat(User user, string chatId);
        Task AddUserToDepartmentChat(User user);
    }
}
