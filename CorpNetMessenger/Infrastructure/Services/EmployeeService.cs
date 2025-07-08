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

        public async Task<IEnumerable<ContactViewModel>> SearchEmployees(
            string term,
            int departmentId,
            string userId
        )
        {
            List<ContactViewModel> contacts;

            if (string.IsNullOrWhiteSpace(term))
            {
                contacts = await _unitOfWork.Users.GetAllDepartmentContactsAsync(userId);
            }
            else
            {
                var search = await _unitOfWork.Users.SearchContactsByNameAsync(term, departmentId);
                contacts = _mapper.Map<List<ContactViewModel>>(search);
            }

            var currentUser = contacts.FirstOrDefault(u => u.Id == userId);
            if (currentUser != null)
            {
                contacts.Remove(currentUser);
            }
            else
            {
                _logger.LogWarning("Текущий пользователь не найден в списке контактов");
            }

            return contacts;
        }
    }
}
