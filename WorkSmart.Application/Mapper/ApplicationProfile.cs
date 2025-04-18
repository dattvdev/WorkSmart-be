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
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.User.Address))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.User.DateOfBirth))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User.Avatar))

                .ForMember(dest => dest.RejectionReason, opt => opt.MapFrom(src => src.RejectionReason))
                .ForMember(dest => dest.ApplicationStatus, opt => opt.MapFrom(src => src.Status))

                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Job.User.CompanyName))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Job.Title))
                .ForMember(dest => dest.CategoryID, opt => opt.MapFrom(src => src.Job.CategoryID))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Job.Description))
                .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Job.Level))
                .ForMember(dest => dest.Education, opt => opt.MapFrom(src => src.Job.Education))
                .ForMember(dest => dest.NumberOfRecruitment, opt => opt.MapFrom(src => src.Job.NumberOfRecruitment))
                .ForMember(dest => dest.WorkType, opt => opt.MapFrom(src => src.Job.WorkType))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Job.Location))
                .ForMember(dest => dest.JobPosition, opt => opt.MapFrom(src => src.Job.JobPosition))
                .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.Job.Salary))
                .ForMember(dest => dest.Exp, opt => opt.MapFrom(src => src.Job.Exp))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Job.Priority))
                .ForMember(dest => dest.Deadline, opt => opt.MapFrom(src => src.Job.Deadline))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Job.Status))
                .ReverseMap();
        }
    }
}
