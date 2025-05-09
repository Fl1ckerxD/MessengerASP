using AutoMapper;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Web.ViewModels;

namespace CorpNetMessenger.Domain.MappingProfiles
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            CreateMap<RegisterViewModel, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Login));
        }
    }
}
