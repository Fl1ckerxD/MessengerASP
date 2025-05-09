using AutoMapper;
using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace CorpNetMessenger.Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountService(IMapper mapper,UserManager<User> userManager, 
            SignInManager<User> signInManager)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<SignInResult> Login(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(
                model.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);
        }

        public void Logout(RegisterDTO user)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityResult> Register(RegisterViewModel model)
        {
            var user = _mapper.Map<User>(model);
            return await _userManager.CreateAsync(user, model.Password);
        }
    }
}
