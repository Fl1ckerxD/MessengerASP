using AutoMapper;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                var result = await _signInManager.PasswordSignInAsync(
                    model.UserName,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.UserName);
                    await UpdateFullNameClaim(user);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for {UserName}", model.UserName);
                return SignInResult.Failed;
            }
        }

        private async Task UpdateFullNameClaim(User user)
        {
            var existingClaims = await _userManager.GetClaimsAsync(user);
            var fullNameClaim = existingClaims.FirstOrDefault(c => c.Type == "FullName");

            var currentFullName = $"{user.LastName} {user.Name}";

            if (fullNameClaim == null)
            {
                await _userManager.AddClaimAsync(user, new Claim("FullName", currentFullName));
            }
            else if (fullNameClaim.Value != currentFullName)
            {
                await _userManager.ReplaceClaimAsync(user, fullNameClaim,
                    new Claim("FullName", currentFullName));
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
                    return IdentityResult.Failed(new IdentityError { Description = "Такая почта уже используется" });

                var user = _mapper.Map<User>(model);
                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Code));
                    _logger.LogWarning("Registration failed for {Username}. Errors: {Errors}",
                    user.UserName, errors);
                }
                else
                {
                    _logger.LogInformation("New user registered: {Username}", user.UserName);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error for user: {Login}", model.Login);
                return IdentityResult.Failed(new IdentityError { Description = "Internal server error" });
            }
        }
    }
}
