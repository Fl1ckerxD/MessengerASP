using AutoMapper;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Domain.MappingProfiles;
using CorpNetMessenger.Infrastructure.Services;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;

namespace CorpNetMessenger.Tests.Services
{
    public class EmployeeServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<EmployeeService>> _mockLogger;
        private readonly IMapper _mapper;
        private readonly EmployeeService _employeeService;

        public EmployeeServiceTests()
        {
            _mockUnitOfWork = new();
            _mockLogger = new();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AppMappingProfile()); 
            });
            _mapper = config.CreateMapper();

            _employeeService = new(_mockUnitOfWork.Object, _mockLogger.Object, _mapper);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetEmployeeInfo_WhenIdIsInvalid_ThrowsArgumentNullException(string? id)
        {
            if (id == null)
            {
                await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    _employeeService.GetEmployeeInfoAsync(id!)
                );
                return;
            }
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _employeeService.GetEmployeeInfoAsync(id)
            );
        }

        [Fact]
        public async Task GetEmployeeInfo_WhenEmployeeNotFound()
        {
            _mockUnitOfWork
                .Setup(u => u.Users.GetByIdWithDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _employeeService.GetEmployeeInfoAsync("id"));
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
                .Setup(u => u.Users.GetAllDepartmentContactsAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedContacts);

            var result = await _employeeService.SearchEmployeesAsync("", departmentId, userId);

            Assert.Equal(expectedContacts.Count - 1, result.Count());
            _mockUnitOfWork.Verify(u => u.Users.GetAllDepartmentContactsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(
                u => u.Users.SearchContactsByNameAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
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
                .Setup(u => u.Users.SearchContactsByNameAsync(term, departmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(searchResults);

            var result = await _employeeService.SearchEmployeesAsync(term, departmentId, userId);

            Assert.Single(result);
            Assert.Equal("user2", result.First().Id);
            _mockUnitOfWork.Verify(
                u => u.Users.SearchContactsByNameAsync(term, departmentId, It.IsAny<CancellationToken>()),
                Times.Once
            );
            _mockUnitOfWork.Verify(
                u => u.Users.GetAllDepartmentContactsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact]
        public async Task SearchEmployees_WithTermAndNoMatches_ReturnsEmptyList()
        {
            var term = "nonexistent";
            var departmentId = 1;
            var userId = "user1";
            var emptySearchResults = new List<User>();

            _mockUnitOfWork
                .Setup(u => u.Users.SearchContactsByNameAsync(term, departmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptySearchResults);

            var result = await _employeeService.SearchEmployeesAsync(term, departmentId, userId);

            Assert.Empty(result);
        }
    }
}
