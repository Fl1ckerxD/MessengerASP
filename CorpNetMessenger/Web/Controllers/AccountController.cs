﻿using CorpNetMessenger.Application.Common;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CorpNetMessenger.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(ILogger<AccountController> logger, UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Edit()
        {
            var currentUser = await _userManager.GetUserAsync(User) ??
                throw new Exception("Пользователь не найден");

            return View(new AccountViewModel
            {
                LastName = currentUser.LastName,
                Name = currentUser.Name,
                Patronymic = currentUser.Patronymic,
                Email = currentUser.Email,
                PhoneNumber = currentUser.PhoneNumber
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AccountViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var currentUser = await _userManager.GetUserAsync(User) ??
                throw new Exception("Пользователь не найден");

            try
            {
                // Обновление основныйх полей
                currentUser.LastName = model.LastName;
                currentUser.Name = model.Name;
                currentUser.Patronymic = model.Patronymic;
                currentUser.PhoneNumber = model.PhoneNumber;

                // Обновление аватарки с проверкой
                if (model.AvatarFile != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.AvatarFile.CopyToAsync(memoryStream);

                        // Проверка, что файл - изображение
                        if (!FileHelper.IsImage(model.AvatarFile.FileName))
                        {
                            ModelState.AddModelError(nameof(model.AvatarFile), "Недопустимый формат изображения");
                            return View(model);
                        }

                        currentUser.Image = memoryStream.ToArray();
                        currentUser.ImageContentType = model.AvatarFile.ContentType;
                    }
                }

                // Обновление email с проверкой
                if (currentUser.Email != model.Email)
                {
                    var emailExists = await _userManager.FindByEmailAsync(model.Email) != null;
                    if (emailExists)
                    {
                        ModelState.AddModelError(nameof(model.Email), "Email уже занят");
                        return View(model);
                    }

                    currentUser.Email = model.Email;
                    currentUser.EmailConfirmed = false;
                }

                // Обновление пароля
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    if (!await _userManager.CheckPasswordAsync(currentUser, model.Password))
                    {
                        ModelState.AddModelError(nameof(model.Password), "Неверный текущий пароль");
                        return View(model);
                    }

                    var result = await _userManager.ChangePasswordAsync(
                        currentUser, model.Password, model.NewPassword);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }
                }

                // Сохранение изменений
                var updateResult = await _userManager.UpdateAsync(currentUser);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                // Обновление аутентифкационных кук
                await _userManager.UpdateSecurityStampAsync(currentUser);
                await _signInManager.RefreshSignInAsync(currentUser);

                _logger.LogInformation("Пользователь {UserId} обновил профиль", currentUser.Id);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении профиля пользователя {UserId}", currentUser.Id);
                ModelState.AddModelError(string.Empty, "Произошла ошибка при обновлении профиля");
                return View(model);
            }
        }

        [HttpGet("Avatar/{userId}")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetAvatar(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user?.Image == null)
                return File("~/images/default-avatar.jpg", "image/jpeg");

            return File(user.Image, user.ImageContentType ?? "image/jpeg");
        }
    }
}
