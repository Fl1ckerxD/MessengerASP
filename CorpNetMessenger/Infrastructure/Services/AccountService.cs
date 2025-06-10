using AutoMapper;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CorpNetMessenger.Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IMapper mapper, UserManager<User> userManager,
            SignInManager<User> signInManager, ILogger<AccountService> logger)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<SignInResult> Login(LoginViewModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user == null)
                    return SignInResult.Failed;

                var result = await _signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var fullName = $"{user.LastName} {user.Name}";

                    // Добавляем claim
                    var claims = new List<Claim> { new Claim("FullName", fullName) };
                    await _userManager.AddClaimsAsync(user, claims);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                return SignInResult.Failed;
            }
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> Register(RegisterViewModel model)
        {
            try
            {
                if (await _userManager.FindByEmailAsync(model.Email) != null)
                    return IdentityResult.Failed(new IdentityError { Description = "Email already exists" });

                var user = _mapper.Map<User>(model);
                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    var error = result.Errors.FirstOrDefault()?.Description;
                    _logger.LogWarning("Registration failed: {Error}", error);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                return IdentityResult.Failed(new IdentityError { Description = "Internal server error" });
            }
        }
    }
}
