using CorpNetMessenger.Web.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken = default);
        Task<SignInResult> LoginAsync(LoginViewModel user, CancellationToken cancellationToken = default);
        Task LogoutAsync();
    }
}
