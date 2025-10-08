using CorpNetMessenger.Application.Configs;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;

namespace CorpNetMessenger.Infrastructure.Services
{
    public class RequestService : IRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatService _chatService;
        private readonly ILogger<RequestService> _logger;

        public RequestService(IUnitOfWork unitOfWork, IChatService chatService, ILogger<RequestService> logger)
        {
            _unitOfWork = unitOfWork;
            _chatService = chatService;
            _logger = logger;
        }

        /// <summary>
        /// Одобряет нового пользователя, активируя его и добавляя в чат отдела
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <exception cref="InvalidOperationException">Если пользователь не найден</exception>
        public async Task AcceptNewUserAsync(string userId)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(userId, nameof(userId));
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new InvalidOperationException($"Пользователя с ID {userId} не существует");

                if (user.StatusId == StatusTypes.Active)
                {
                    _logger.LogInformation("Попытка повторного одобрения пользователя {UserId}", userId);
                    return;
                }

                user.StatusId = StatusTypes.Active;
                await _chatService.AddUserToDepartmentChatAsync(user);
                _logger.LogInformation("Пользователь {UserId} успешно одобрен и добавлен в чат", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при одобрении пользователя {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Отклоняет запрос пользователя, устанавливая статус "Отклонен"
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <exception cref="InvalidOperationException">Если пользователь не найден</exception>
        public async Task RejectNewUserAsync(string userId)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(userId, nameof(userId));
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new InvalidOperationException($"Пользователя с ID {userId} не существует");

                if (user.StatusId == StatusTypes.Rejected)
                {
                    _logger.LogInformation("Попытка повторного отклонения пользователя {UserId}", userId);
                    return;
                }

                user.StatusId = StatusTypes.Rejected;
                await _unitOfWork.SaveAsync();
                _logger.LogInformation("Пользователь {UserId} отклонён", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отклонении пользователя {UserId}", userId);
                throw;
            }
        }
    }
}