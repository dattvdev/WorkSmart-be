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
                .ForMember(u => u.Email, option => option.MapFrom(a => a.Job.User.Email))
                .ForMember(dest => dest.RejectionReason, opt => opt.MapFrom(src => src.RejectionReason))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Job.User.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Job.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Job.User.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.User.Address))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.Job.User.DateOfBirth))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Job.User.Gender))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Job.User.Avatar))
                .ForMember(dest => dest.ApplicationStatus, opt => opt.MapFrom(src => src.Status))
                //.ForMember(dest => dest.CV_Skills, opt => opt.MapFrom(src => src.CV.Skills))
                //.ForMember(dest => dest.CV_Educations, opt => opt.MapFrom(src => src.CV.Educations))
                //.ForMember(dest => dest.CV_Experiences, opt => opt.MapFrom(src => src.CV.Experiences))
                //job
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
