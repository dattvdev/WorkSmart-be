using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.AccountDtos;
using WorkSmart.Core.Dto.CVDtos;
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

            // Fix the mapping for AccountWithFeaturedCVDto
            CreateMap<User, AccountWithFeaturedCVDto>()
                .ForMember(dest => dest.FeaturedCVs, opt => opt.MapFrom(src =>
                    src.CVs.Where(cv => cv.IsFeatured == true)));

            CreateMap<CV, FeaturedCVDto>();
            CreateMap<CV_Certification, CVCertificationDto>()
                   .ForMember(dest => dest.CertificateName, opt => opt.MapFrom(src => src.CertificateName))
                   .ForMember(dest => dest.CreateAt, opt => opt.MapFrom(src => src.CreateAt))
                   .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                   .ReverseMap();
            CreateMap<CV_Experience, CVExperienceDto>()
                   .ForMember(dest => dest.JobPosition, opt => opt.MapFrom(src => src.JobPosition))
                   .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
                   .ForMember(dest => dest.StartedAt, opt => opt.MapFrom(src => src.StartedAt))
                   .ForMember(dest => dest.EndedAt, opt => opt.MapFrom(src => src.EndedAt))
                   .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                   .ReverseMap();
            CreateMap<CV_Skill, CVSkillDto>().ReverseMap();
            CreateMap<CV_Education, CVEducationDto>()
                    .ForMember(dest => dest.SchoolName, opt => opt.MapFrom(src => src.SchoolName))
                    .ForMember(dest => dest.Degree, opt => opt.MapFrom(src => src.Degree))
                    .ForMember(dest => dest.StartedAt, opt => opt.MapFrom(src => src.StartedAt))
                    .ForMember(dest => dest.EndedAt, opt => opt.MapFrom(src => src.EndedAt))
                    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                    .ReverseMap();
        }
    }
}
