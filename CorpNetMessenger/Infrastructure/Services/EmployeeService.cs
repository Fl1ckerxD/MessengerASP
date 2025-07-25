using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.Interfaces.Services;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CorpNetMessenger.Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IMapper _mapper;

        public EmployeeService(
            IUnitOfWork unitOfWork,
            ILogger<EmployeeService> logger,
            IMapper mapper
        )
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
        /// <exception cref="Exception">Если сотрудник не найден</exception>
        public async Task<EmployeeDto> GetEmployeeInfo(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException("ID не может быть пустым");

            var employee = await _unitOfWork.Users.GetByIdWithDetailsAsync(id);
            if (employee == null)
                throw new Exception($"Сотрудник [{id}] не найден");

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
        public async Task<IEnumerable<ContactViewModel>> SearchEmployees(
            string term,
            int departmentId,
            string userId
        )
        {
            List<ContactViewModel> filteredContacts;

            if (string.IsNullOrWhiteSpace(term))
            {
                var contacts = await _unitOfWork.Users.GetAllDepartmentContactsAsync(userId);
                filteredContacts = contacts.Where(u => u.Id != userId).ToList();
            }
            else
            {
                var search = await _unitOfWork.Users.SearchContactsByNameAsync(term, departmentId);
                var filteredSearch = search.Where(u => u.Id != userId).ToList();
                filteredContacts = _mapper.Map<List<ContactViewModel>>(filteredSearch);
            }

            return filteredContacts;
        }
    }
}
