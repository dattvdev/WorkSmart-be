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
                .ForMember(dest => dest.ReportStatus, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ReportContent, opt => opt.MapFrom(src => src.Content));

            CreateMap<User, AccountDto>();
            CreateMap<Job, JobDto>();

        }
    }
}
