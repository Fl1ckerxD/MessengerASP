namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IChatService
    {
        Task SaveMessage(string content, string userId, string chatId);
    }
}
