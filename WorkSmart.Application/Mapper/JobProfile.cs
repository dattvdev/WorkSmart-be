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

            // Mapping CreateJobDto,Job
            CreateMap<CreateJobDto, Job>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => JobStatus.Pending));

            // Mapping UpdateJobDto,Job
            CreateMap<UpdateJobDto, Job>();
            CreateMap<Job, GetListSearchJobDto>().ReverseMap(); ;
            CreateMap<Job, ExpiredJobDto>()
                .ForMember(dest => dest.HiddenAt, opt => opt.MapFrom(src => DateTime.Now));

        }
    }
}
