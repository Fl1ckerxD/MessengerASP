﻿using AutoMapper;
using CorpNetMessenger.Application.Common;
using CorpNetMessenger.Application.Converters;
using CorpNetMessenger.Domain.DTOs;
using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Web.Areas.Admin.ViewModels;
using CorpNetMessenger.Web.Areas.Messaging.ViewModels;
using CorpNetMessenger.Web.ViewModels;

namespace CorpNetMessenger.Domain.MappingProfiles
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            CreateMap<RegisterViewModel, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Login));

            CreateMap<Message, MessageDto>()
               .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Content))
               .ForMember(dest => dest.SentAt, opt => opt.MapFrom(src => src.SentAt.ToString("dd.MM.yyyy HH:mm:ss")));

            CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.LastName} {src.Name}"));

            CreateMap<Attachment, AttachmentDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => BytesToStringConverter.Convert(src.FileLength)))
                .ForMember(dest => dest.IsImage, opt => opt.MapFrom(src => FileHelper.IsImage(src.FileName)));

            CreateMap<User, ContactViewModel>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.LastName} {src.Name}"))
                .ForMember(dest => dest.PostName, opt => opt.MapFrom(src => src.Post.Title));

            CreateMap<User, EmployeeDto>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Title))
                .ForMember(dest => dest.PostName, opt => opt.MapFrom(src => src.Post.Title));

            CreateMap<User, RequestViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.LastName} {src.Name} {src.Patronymic}"))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Title))
                .ForMember(dest => dest.PostName, opt => opt.MapFrom(src => src.Post.Title));

            CreateMap<User, UsersTableViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.LastName} {src.Name} {src.Patronymic}"))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Title))
                .ForMember(dest => dest.PostName, opt => opt.MapFrom(src => src.Post.Title))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Title));
        }
    }
}