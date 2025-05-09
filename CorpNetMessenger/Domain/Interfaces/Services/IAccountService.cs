using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Web.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IAccountService
    {
        Task<IdentityResult> Register(RegisterViewModel model);
        Task<SignInResult> Login(LoginViewModel user);
        void Logout(RegisterDTO user);
    }
}
