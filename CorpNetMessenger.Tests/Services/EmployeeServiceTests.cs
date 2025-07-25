using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Infrastructure.Services;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CorpNetMessenger.Tests.Services
{
    public class EmployeeServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<EmployeeService>> _mockLogger;
        private readonly Mock<IMapper> _mockMapper;
        private readonly EmployeeService _employeeService;

        public EmployeeServiceTests()
        {
            _mockUnitOfWork = new();
            _mockLogger = new();
            _mockMapper = new();

            _employeeService = new(_mockUnitOfWork.Object, _mockLogger.Object, _mockMapper.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetEmployeeInfo_WhenIdIsInvalid_ThrowsArgumentNullException(string? id)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _employeeService.GetEmployeeInfo(id)
            );
        }

        [Fact]
        public async Task GetEmployeeInfo_WhenEmployeeNotFound()
        {
            _mockUnitOfWork
                .Setup(u => u.Users.GetByIdWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);
            await Assert.ThrowsAsync<Exception>(() => _employeeService.GetEmployeeInfo("id"));
        }

        [Fact]
        public async Task SearchEmployees_EmptyTerm_ReturnsAllDepartmentContacts()
        {
            var userId = "currentUserId";
            var departmentId = 1;
            var expectedContacts = new List<ContactViewModel>
            {
                new ContactViewModel { Id = "user1" },
                new ContactViewModel { Id = "user2" },
                new ContactViewModel { Id = "currentUserId" },
            };

            _mockUnitOfWork
                .Setup(u => u.Users.GetAllDepartmentContactsAsync(userId))
                .ReturnsAsync(expectedContacts);

            var result = await _employeeService.SearchEmployees("", departmentId, userId);

            Assert.Equal(expectedContacts.Count - 1, result.Count());
            _mockUnitOfWork.Verify(u => u.Users.GetAllDepartmentContactsAsync(userId), Times.Once);
            _mockUnitOfWork.Verify(
                u => u.Users.SearchContactsByNameAsync(It.IsAny<string>(), It.IsAny<int>()),
                Times.Never
            );
        }

        [Fact]
        public async Task SearchEmployees_WithTerm_ReturnsFilteredContacts()
        {
            var term = "test";
            var departmentId = 1;
            var userId = "user1";
            var searchResults = new List<User>
            {
                new User { Id = "user1" },
                new User { Id = "user2" },
            };
            var expectedViewModels = new List<ContactViewModel>
            {
                new ContactViewModel { Id = "user2" },
            };

            _mockUnitOfWork
                .Setup(u => u.Users.SearchContactsByNameAsync(term, departmentId))
                .ReturnsAsync(searchResults);
            _mockMapper
                .Setup(m => m.Map<List<ContactViewModel>>(It.IsAny<List<User>>()))
                .Returns(expectedViewModels);

            var result = await _employeeService.SearchEmployees(term, departmentId, userId);

            Assert.Single(result);
            Assert.Equal("user2", result.First().Id);
            _mockUnitOfWork.Verify(
                u => u.Users.SearchContactsByNameAsync(term, departmentId),
                Times.Once
            );
            _mockUnitOfWork.Verify(
                u => u.Users.GetAllDepartmentContactsAsync(It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task SearchEmployees_EmptyTermAndEmptyContacts_ReturnsEmptyList()
        {
            var userId = "user1";
            var departmentId = 1;
            var emptyContacts = new List<ContactViewModel>();

            _mockUnitOfWork
                .Setup(u => u.Users.GetAllDepartmentContactsAsync(userId))
                .ReturnsAsync(emptyContacts);

            var result = await _employeeService.SearchEmployees("", departmentId, userId);

            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchEmployees_WithTermAndNoMatches_ReturnsEmptyList()
        {
            var term = "nonexistent";
            var departmentId = 1;
            var userId = "user1";
            var emptySearchResults = new List<User>();

            _mockUnitOfWork
                .Setup(u => u.Users.SearchContactsByNameAsync(term, departmentId))
                .ReturnsAsync(emptySearchResults);
            _mockMapper
                .Setup(m => m.Map<List<ContactViewModel>>(emptySearchResults))
                .Returns(new List<ContactViewModel>());

            var result = await _employeeService.SearchEmployees(term, departmentId, userId);

            Assert.Empty(result);
        }
    }
}
