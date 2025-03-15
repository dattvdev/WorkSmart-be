using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.ApplicationDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Application.Mapper
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {

            CreateMap<Core.Entity.Application, ApplicationJobDto>()
                .ForMember(u => u.FullName, option => option.MapFrom(a => a.User.FullName))
                .ForMember(u => u.PhoneNumber, option => option.MapFrom(a => a.User.PhoneNumber))
                .ForMember(u => u.Email, option => option.MapFrom(a => a.User.Email))
                .ForMember(dest => dest.RejectionReason, opt => opt.MapFrom(src => src.RejectionReason))
            .ForMember(dest => dest.RejectionReason, opt => opt.MapFrom(src => src.RejectionReason))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.User.Address))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.User.DateOfBirth))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender))
            .ReverseMap();

        }
    }
}
