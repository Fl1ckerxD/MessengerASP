using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.Views.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CorpNetMessenger.Web.Controllers
{
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
                await _hubContext.Clients.Group(request.ChatId).SendAsync("Receive", request.Text, attachmentsDto, user.Identity.Name, DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка сохранения сообщения в бд");
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
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);

                var attachment = new Attachment
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    FileData = memoryStream.ToArray(),
                    FileLength = file.Length
                };
                result.Add(attachment);
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
