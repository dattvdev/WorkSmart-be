using AutoMapper;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;

namespace WorkSmart.Application.Mapper
{
    public class JobProfile : Profile
    {
        public JobProfile()
        {

            CreateMap<Job, JobDto>();
            CreateMap<Tag, JobDetailTagDto>();
            CreateMap<Job, JobDetailDto>()
                .ForMember(dest => dest.CompanySize, opt => opt.MapFrom(src => src.User.CompanySize))
                .ForMember(dest => dest.CompanyWebsite, opt => opt.MapFrom(src => src.User.CompanyWebsite))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.User.Address))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.User.CompanyName))
                .ForMember(dest => dest.CompanyDescription, opt => opt.MapFrom(src => src.User.CompanyDescription))
                .ForMember(dest => dest.WorkLocation, opt => opt.MapFrom(src => src.User.WorkLocation))
                .ForMember(dest => dest.Industry, opt => opt.MapFrom(src => src.User.Industry))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(t => t.TagID).ToList()))
                .ForMember(dest => dest.JobDetailTags, opt => opt.MapFrom(src => src.Tags))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User.Avatar));
            // Mapping CreateJobDto,Job
            CreateMap<CreateJobDto, Job>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => JobStatus.Pending));

            // Mapping UpdateJobDto,Job
            CreateMap<UpdateJobDto, Job>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now));
            CreateMap<Job, GetListSearchJobDto>()
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User.Avatar));
            CreateMap<Job, ExpiredJobDto>()
                .ForMember(dest => dest.HiddenAt, opt => opt.MapFrom(src => DateTime.Now));

        }
    }
}
