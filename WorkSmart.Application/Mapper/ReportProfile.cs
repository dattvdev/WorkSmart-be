using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.AdminDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Application.Mapper
{
    public class ReportProfile : Profile
    {
        public ReportProfile()
        {
            CreateMap<ReportPost, ReportListDto>()
            .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender.FullName))
            .ForMember(dest => dest.SenderAvatar, opt => opt.MapFrom(src => src.Sender.Avatar))
            .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.Job.Title))
            .ForMember(dest => dest.ReportTitle, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.ReportContent, opt => opt.MapFrom(src => src.Content));
        }
    }
}
