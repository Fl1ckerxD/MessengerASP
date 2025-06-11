using AutoMapper;
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
        private readonly IMapper _mapper;
        public ChatService(IUnitOfWork unitOfWork, ILogger<ChatService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Сохранение сообщения в БД
        /// </summary>
        /// <param name="content">Текст сообщения</param>
        /// <param name="userId">Id пользователя отправившего сообщение</param>
        /// <param name="chatId">Чат в который было отправлено сообщение</param>
        /// <returns>Возвращает Id сохраненного сообщения</returns>
        public async Task<string> SaveMessage(ChatMessageDto request, string userId)
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

                return message.Id;
            }
            catch (Exception ex)
            {
                string er = "Ошибка при сохранении сообщения";
                _logger.LogError(ex, er);
                throw new Exception(er);
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

        /// <summary>
        /// Получает сообщение по его идентификатору и преобразует в DTO для передачи клиенту.
        /// </summary>
        /// <param name="messageId">Идентификатор сообщения</param>
        /// <returns>Объект <see cref="MessageDto"/>, представляющий сообщение с дополнительными данными</returns>
        /// <exception cref="ArgumentNullException">Если сообщение с указанным ID не найдено</exception>
        public async Task<MessageDto> GetMessageAsync(string messageId)
        {
            var messageEntity = await _unitOfWork.Messages.GetMessageWithDetailsAsync(messageId);
            if (messageEntity == null)
                throw new ArgumentNullException(nameof(messageId), "Сообщение не найдено");

            var messageDto = _mapper.Map<MessageDto>(messageEntity);
            return messageDto;
        }

        public async Task<IEnumerable<MessageDto>> LoadHistoryChatAsync(string chatId, int skip = 0, int take = 5)
        {
            if (!await _unitOfWork.Chats.AnyAsync(c => c.Id == chatId)) // Проверка наличия чата с таким id
                throw new Exception("Такого чата нет");

            var messages = _unitOfWork.Messages.LoadHistoryChatAsync(chatId, skip, take);
            return _mapper.Map<List<MessageDto>>(messages.Result);
        }
    }
}
