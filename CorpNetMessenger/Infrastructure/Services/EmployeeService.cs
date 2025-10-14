using AutoMapper;
using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;

namespace CorpNetMessenger.Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IMapper _mapper;

        public EmployeeService(IUnitOfWork unitOfWork, ILogger<EmployeeService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Получает подробную информацию о сотруднике по его идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сотрудника</param>
        /// <returns>DTO с информацией о сотруднике</returns>
        /// <exception cref="ArgumentNullException">Если id пустой или null</exception>
        /// <exception cref="KeyNotFoundException">Если сотрудник не найден</exception>
        public async Task<EmployeeDto> GetEmployeeInfoAsync(string id, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(id, nameof(id));

            var employee = await _unitOfWork.Users.GetByIdWithDetailsAsync(id, cancellationToken);
            if (employee == null)
            {
                _logger.LogWarning("Сотрудник с id {EmployeeId} не найден", id);
                throw new KeyNotFoundException($"Сотрудник [{id}] не найден");
            }

            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            return employeeDto;
        }

        /// <summary>
        /// Поиск сотрудников по имени в рамках отдела с исключением текущего пользователя
        /// </summary>
        /// <param name="term">Строка поиска (ФИО, Должность сотрудника)</param>
        /// <param name="departmentId">Идентификатор отдела</param>
        /// <param name="userId">Идентификатор текущего пользователя (будет исключен из результатов)</param>
        /// <returns>Список контактов сотрудников</returns>
        public async Task<IEnumerable<ContactViewModel>> SearchEmployeesAsync(string term, int departmentId, string userId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(userId, nameof(userId));

            List<ContactViewModel> filteredContacts;

            if (string.IsNullOrWhiteSpace(term))
            {
                var contacts = await _unitOfWork.Users.GetAllDepartmentContactsAsync(userId, cancellationToken);
                filteredContacts = _mapper.Map<List<ContactViewModel>>(contacts)
                .Where(c => c.Id != userId)
                .ToList();
            }
            else
            {
                var search = await _unitOfWork.Users.SearchContactsByNameAsync(term, departmentId, cancellationToken);
                filteredContacts = _mapper.Map<List<ContactViewModel>>(search)
                .Where(c => c.Id != userId)
                .ToList();
            }

            return filteredContacts;
        }
    }
}
