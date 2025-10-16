namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IRequestService
    {
        Task AcceptNewUserAsync(string userId, CancellationToken cancellationToken = default);
        Task RejectNewUserAsync(string userId, CancellationToken cancellationToken = default);
    }
}