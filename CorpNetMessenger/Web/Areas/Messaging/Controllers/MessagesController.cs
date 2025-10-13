using CorpNetMessenger.Application.Configs;
using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CorpNetMessenger.Web.Areas.Messaging.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<MessagesController> _logger;
        private readonly IChatService _chatService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly IChatCacheService _chatCacheService;
        private readonly IMessageService _messageService;
        private readonly IUserContext _userContext;

        public MessagesController(IHubContext<ChatHub> hubContext, ILogger<MessagesController> logger,
            IChatService chatService, IUnitOfWork unitOfWork,
            IFileService fileService, IChatCacheService chatCacheService,
            IMessageService messageService, IUserContext userContext)
        {
            _logger = logger;
            _chatService = chatService;
            _hubContext = hubContext;
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _chatCacheService = chatCacheService;
            _messageService = messageService;
            _userContext = userContext;
        }

        [HttpPost("send")]
        [RequestSizeLimit(MessagingOptions.MaxFileSize)]
        [RequestFormLimits(MultipartBodyLengthLimit = MessagingOptions.MaxFileSize, ValueCountLimit = MessagingOptions.MaxFileCount)]
        public async Task<IActionResult> Send()
        {
            var form = await Request.ReadFormAsync();

            var message = form["message"].ToString();
            var chatId = form["chatId"].ToString();
            var files = form.Files;

            if (!await _chatService.UserInChatAsync(chatId, _userContext.UserId))
                return Forbid("Вы не состоите в этом чате");

            if (string.IsNullOrWhiteSpace(message) && !files.Any())
                return BadRequest("Сообщение не может быть пустым");

            if (message.Length > MessagingOptions.MaxMessageLength)
                return BadRequest($"Сообщение превышает {MessagingOptions.MaxMessageLength} символов");

            try
            {
                var attachments = await _fileService.ProcessFilesAsync(files);
                var chatMessageDto = new ChatMessageDto
                {
                    ChatId = chatId,
                    Text = message,
                    Files = attachments
                };

                var messageDto = await _messageService.SaveMessageAsync(chatMessageDto, _userContext.UserId);

                _chatCacheService.InvalidateChatCache(chatId);

                await _hubContext.Clients.Group(chatId)
                    .SendAsync("Receive", messageDto);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки сообщения в чат {chatId}", chatId);
                return StatusCode(500, "Ошибка обработки сообщения");
            }
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(string id)
        {
            var attachment = await _unitOfWork.Files.GetByIdAsync(id);

            if (attachment == null)
                return NotFound();

            return File(attachment.FileData, attachment.ContentType, attachment.FileName);
        }
    }
}
