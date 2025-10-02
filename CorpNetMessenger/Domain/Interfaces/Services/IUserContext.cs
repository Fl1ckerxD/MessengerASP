namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IUserContext
    {
        string? UserId { get; }
        string? Role { get; }
        bool IsAuthenticated { get; }
    }
}