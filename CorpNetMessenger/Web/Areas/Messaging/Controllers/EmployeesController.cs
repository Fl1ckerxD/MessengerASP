using CorpNetMessenger.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorpNetMessenger.Web.Areas.Messaging.Controllers
{
    [Area("Messaging")]
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly ILogger<EmployeesController> _logger;
        private readonly IEmployeeService _employeeService;

        public EmployeesController(
            ILogger<EmployeesController> logger,
            IEmployeeService employeeService
        )
        {
            _logger = logger;
            _employeeService = employeeService;
        }

        [HttpGet]
        [ResponseCache(Duration = 360, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetEmployeeInfo(string id)
        {
            try
            {
                return Json(await _employeeService.GetEmployeeInfo(id));
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка получения информации о сотруднике", ex);
                return NotFound();
            }
        }
    }
}
