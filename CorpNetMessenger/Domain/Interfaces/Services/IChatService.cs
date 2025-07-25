﻿using CorpNetMessenger.Application.Common;
using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Entities;

namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IChatService
    {
        Task<string> SaveMessage(ChatMessageDto request, string userId);
        Task<OperationResult> EditMessage(string messageId, string newText, string userId);
        Task<OperationResult> DeleteMessage(string messageId, string userId);
        Task<bool> UserInChat(string chatId, string userId);
        Task<MessageDto> GetMessageAsync(string messageId);
        Task<IEnumerable<MessageDto>> LoadHistoryChatAsync(string chatId, int skip = 0, int take = 5);
        Task<Chat> GetDepartmentChatForUserAsync(string userId);
        Task AddUserToChat(string userId, string chatId);
        Task AddUserToChat(User user, Chat chat);
        Task AddUserToChat(User user, string chatId);
        Task AddUserToDepartmentChat(User user);
    }
}
