using CorpNetMessenger.Application.Configs;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;

namespace CorpNetMessenger.Infrastructure.Services
{
    public class RequestService : IRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatService _chatService;

        public RequestService(IUnitOfWork unitOfWork, IChatService chatService)
        {
            _unitOfWork = unitOfWork;
            _chatService = chatService;
        }

        /// <summary>
        /// Одобряет нового пользователя, активируя его и добавляя в чат отдела
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <exception cref="InvalidOperationException">Если пользователь не найден</exception>
        public async Task AcceptNewUserAsync(string userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException($"Пользователя с ID {userId} не существует");

            user.StatusId = StatusTypes.Active;
            await _chatService.AddUserToDepartmentChatAsync(user);
        }

        /// <summary>
        /// Отклоняет запрос пользователя, устанавливая статус "Отклонен"
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <exception cref="InvalidOperationException">Если пользователь не найден</exception>
        public async Task RejectNewUserAsync(string userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException($"Пользователя с ID {userId} не существует");

            user.StatusId = StatusTypes.Rejected;
            await _unitOfWork.SaveAsync();
        }
    }
}