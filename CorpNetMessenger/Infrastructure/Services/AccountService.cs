using AutoMapper;
using CorpNetMessenger.Application.Configs;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
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

        /// <summary>
        /// Аутентификация пользователя
        /// </summary>
        /// <param name="model">Модель с данными для входа</param>
        /// <returns>Результат попытки входа</returns>
        public async Task<SignInResult> Login(LoginViewModel model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.UserName);

                if (user.StatusId != StatusTypes.Active)
                    return SignInResult.NotAllowed;

                var result = await _signInManager.PasswordSignInAsync(
                    model.UserName,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                // Если вход успешен, обновляем claim с полным именем
                if (result.Succeeded)
                {
                    await UpdateFullNameClaim(user);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка входа для {UserName}", model.UserName);
                return SignInResult.Failed;
            }
        }

        /// <summary>
        /// Обновляет claim с полным именем пользователя
        /// </summary>
        /// <param name="user">Пользователь</param>
        public async Task UpdateFullNameClaim(User user)
        {
            var existingClaims = await _userManager.GetClaimsAsync(user);
            var fullNameClaim = existingClaims.FirstOrDefault(c => c.Type == "FullName");

            var currentFullName = $"{user.LastName} {user.Name}";

            // Если claim не существует - добавляем
            if (fullNameClaim == null)
            {
                await _userManager.AddClaimAsync(user, new Claim("FullName", currentFullName));
            }
            // Если существует, но значение изменилось - обновляем
            else if (fullNameClaim.Value != currentFullName)
            {
                await _userManager.ReplaceClaimAsync(user, fullNameClaim,
                    new Claim("FullName", currentFullName));
            }
        }

        /// <summary>
        /// Выход пользователя из системы
        /// </summary>
        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="model">Модель с данными для регистрации</param>
        /// <returns>Результат регистрации</returns>
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
                    _logger.LogWarning("Регистрация не удалась для {Username}. Errors: {Errors}",
                    user.UserName, errors);
                }
                else
                {
                    _logger.LogInformation("Зарегистрирован новый пользователь: {Username}", user.UserName);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка регистрации пользователя: {Login}", model.Login);
                return IdentityResult.Failed(new IdentityError { Description = "Внутренняя ошибка сервера" });
            }
        }
    }
}
