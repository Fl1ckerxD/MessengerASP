using AutoMapper;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CorpNetMessenger.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IActionResult> List(CancellationToken cancellationToken)
        {
            var users = await _unitOfWork.Users.GetAllUserWithDetailsAsync(cancellationToken);
            return View(_mapper.Map<IEnumerable<UsersTableViewModel>>(users));
        }

        public async Task<IActionResult> Requests(CancellationToken cancellationToken)
        {
            var requests = await _unitOfWork.Users.GetAllNewUsersAsync(cancellationToken);
            return View(_mapper.Map<IEnumerable<RequestViewModel>>(requests));
        }
    }
}
