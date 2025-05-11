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

        public async Task SaveMessage(string content, string userId)
        {
            var chatId = "21604bf9-e8d7-41e2-9fc3-0b5f0796f5b3";
            //var chat = new Chat();
            await _unitOfWork.Messages.AddAsync(new Message
            {
                ChatId = chatId,
                Content = content != null ? content.Trim() : "",
                UserId = userId
            });
            await _unitOfWork.SaveAsync();
        }
    }
}
