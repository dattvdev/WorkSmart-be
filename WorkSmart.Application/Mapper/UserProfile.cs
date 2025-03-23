using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.EmployerDtos;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Dto.UserDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Application.Mapper
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserMessageInfoDto>().ReverseMap();
            CreateMap<Job, CompanyJobDto>().ReverseMap();
            CreateMap<User, CompanyDto>()
               .ForMember(dest => dest.PostedJobs, opt => opt.MapFrom(src => src.PostedJobs));
        }
    }
}
