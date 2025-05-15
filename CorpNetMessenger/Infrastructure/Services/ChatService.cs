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
    }
}
