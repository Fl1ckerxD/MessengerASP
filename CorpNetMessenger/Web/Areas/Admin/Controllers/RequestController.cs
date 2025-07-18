using CorpNetMessenger.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorpNetMessenger.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("Admin/[controller]")]
    public class RequestController : Controller
    {
        private readonly IRequestService _reqestService;
        private readonly ILogger<RequestController> _logger;

        public RequestController(IRequestService requestService, ILogger<RequestController> logger)
        {
            _reqestService = requestService;
            _logger = logger;
        }

        [HttpPost("Accept")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptNewUser([FromBody] string userId)
        {
            try
            {
                await _reqestService.AcceptNewUser(userId);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return Conflict(ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Некорректные данные");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось добавить пользователя в систему");
                return StatusCode(500, "Произошла внутренняя ошибка. Обратитесь в поддержку");
            }
        }

        [HttpPost("Reject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectNewUser([FromBody] string userId)
        {
            try
            {
                await _reqestService.RejectNewUser(userId);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Пользователь не найден: {UserId}", userId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось отклонить запрос");
                return StatusCode(500, "Произошла внутренняя ошибка. Обратитесь в поддержку");
            }
        }
    }
}
