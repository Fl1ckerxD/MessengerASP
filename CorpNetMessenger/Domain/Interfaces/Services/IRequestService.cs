namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IRequestService
    {
        Task AcceptNewUser(string userId);
        Task RejectNewUser(string userId);
    }
}