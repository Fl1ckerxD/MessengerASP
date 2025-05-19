using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;

namespace CorpNetMessenger.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ChatService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

        /// <summary>
        /// Редактирование сообщения
        /// </summary>
        /// <param name="messageId">Id конкретного сообщения для редактирования</param>
        /// <param name="newText">Отредактированный текст</param>
        /// <returns>Возвращает true если сообщение было отредактировано иначе false</returns>
        public async Task<bool> EditMessage(string messageId, string newText)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);

            if (message == null) return false;
            if (string.IsNullOrWhiteSpace(newText)) return false;

            message.Content = newText;
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
