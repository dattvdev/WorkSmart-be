using AutoMapper;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;
using WorkSmart.Core.Helpers;

namespace WorkSmart.Application.Mapper
{
    public class JobProfile : Profile
    {
        public JobProfile()
        {

            CreateMap<Job, JobDto>()
                 .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User.Avatar));
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
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => TimeHelper.GetVietnamTime()))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom((src, dest, destMember, context) =>
        context.Items.ContainsKey("Tags") ? context.Items["Tags"] as ICollection<Tag> : new List<Tag>()
    ));
            CreateMap<Job, GetListSearchJobDto>()
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User.Avatar))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.User.CompanyName));
            CreateMap<Job, ExpiredJobDto>()
                .ForMember(dest => dest.HiddenAt, opt => opt.MapFrom(src => TimeHelper.GetVietnamTime()));

            CreateMap<Job, JobActiveDto>()
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.User.CompanyName));
        }
    }
}
