using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

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

        public MessagesController(IHubContext<ChatHub> hubContext, ILogger<MessagesController> logger,
            IChatService chatService, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _chatService = chatService;
            _hubContext = hubContext;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send()
        {
            var form = await Request.ReadFormAsync();

            var message = form["message"].ToString();
            var chatId = form["chatId"].ToString();
            var files = form.Files;

            var attachments = await GetFiles(files);
            var request = new ChatMessageDto
            {
                ChatId = chatId,
                Text = message,
                Files = attachments
            };

            var user = HttpContext.User;
            string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var isInChat = await _chatService.UserInChat(chatId, userId);
            if (!isInChat)
                return StatusCode(403, "Вы не состоите в этом чате");

            if (string.IsNullOrWhiteSpace(message))
                return BadRequest("Сообщение не может быть пустым");

            if (message.Length > 200)
                return BadRequest("Сообщение превышает 200 символов");

            try
            {
                await _chatService.SaveMessage(request, userId);

                var attachmentsDto = new List<AttachmentDto>();
                foreach (var attachment in attachments)
                {
                    attachmentsDto.Add(new AttachmentDto
                    {
                        Id = attachment.Id.ToString(),
                        FileName = attachment.FileName,
                    });
                }
                await _hubContext.Clients.Group(request.ChatId)
                    .SendAsync("Receive", request.Text, attachmentsDto, user.Identity.Name, DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении сообщения");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
            return Ok();
        }

        private async Task<List<Attachment>> GetFiles(IFormFileCollection files)
        {
            var result = new List<Attachment>();
            if (files == null || files.Count == 0)
                return result;

            foreach (var file in files)
            {
                if (file.Length > 10 * 1024 * 1024) // 10 MB
                    throw new ArgumentException("Файл слишком большой");

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);

                result.Add(new Attachment
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    FileData = memoryStream.ToArray(),
                    FileLength = file.Length
                });
            }
            return result;
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
