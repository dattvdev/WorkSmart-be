using AutoMapper;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Application.Mapper
{
    public class JobAlertProfile : Profile
    {
        public JobAlertProfile()
        {
            CreateMap<JobAlert,JobAlertDto>().ReverseMap();
            CreateMap<JobAlertCreateDto,JobAlert>();

        }
    }
}
