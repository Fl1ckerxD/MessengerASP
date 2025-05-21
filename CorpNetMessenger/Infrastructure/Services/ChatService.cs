using CorpNetMessenger.Application.Common;
using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;

namespace CorpNetMessenger.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ChatService> _logger;
        public ChatService(IUnitOfWork unitOfWork, ILogger<ChatService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Сохранение сообщения в БД
        /// </summary>
        /// <param name="content">Текст сообщения</param>
        /// <param name="userId">Id пользователя отправившего сообщение</param>
        /// <param name="chatId">Чат в который было отправлено сообщение</param>
        /// <returns></returns>
        public async Task SaveMessage(ChatMessageDto request, string userId)
        {
            try
            {
                bool chatUser = await UserInChat(request.ChatId, userId);

                if (!chatUser)
                    throw new UnauthorizedAccessException("Пользователь не состоит в чате");

                var message = new Message
                {
                    ChatId = request.ChatId,
                    Content = request.Text != null ? request.Text.Trim() : "",
                    UserId = userId,
                    Attachments = request.Files
                };

                await _unitOfWork.Messages.AddAsync(message);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении сообщения");
            }
        }

        /// <summary>
        /// Редактирование сообщения
        /// </summary>
        /// <param name="messageId">Id конкретного сообщения для редактирования</param>
        /// <param name="newText">Отредактированный текст</param>
        /// <returns>Возвращает true если сообщение было отредактировано иначе false</returns>
        public async Task<OperationResult> EditMessage(string messageId, string newText, string userId)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);

            if (message == null)
                return new OperationResult { Success = false, Error = "Сообщение не найдено" };

            if (message.UserId != userId)
                return new OperationResult { Success = false, Error = "Нельзя редактировать чужое сообщение" };

            if (string.IsNullOrWhiteSpace(newText))
                return new OperationResult { Success = false, Error = "Текст не может быть пустым" };

            if (newText.Length > 200)
                return new OperationResult { Success = false, Error = "Сообщение слишком длинное" };

            message.Content = newText;
            await _unitOfWork.SaveAsync();

            return new OperationResult { Success = true };
        }

        /// <summary>
        /// Проверка состоит ли пользователь в указанном чате
        /// </summary>
        /// <param name="chatId">Id группы</param>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Возвращает true если состоит иначе false</returns>
        public async Task<bool> UserInChat(string chatId, string userId)
        {
            var chatUser = await _unitOfWork.ChatUsers.GetByPredicateAsync(cu => cu.ChatId == chatId && cu.UserId == userId);
            return chatUser != null;
        }
    }
}
