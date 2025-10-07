namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IRequestService
    {
        Task AcceptNewUserAsync(string userId);
        Task RejectNewUserAsync(string userId);
    }
}