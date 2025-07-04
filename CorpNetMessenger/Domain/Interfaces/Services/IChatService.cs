﻿using CorpNetMessenger.Application.Common;
using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;

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
        Task<IEnumerable<ContactViewModel>> SearchEmployees(string term, int departmentId, string userId);
    }
}
