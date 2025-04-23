using AutoMapper;
using WorkSmart.Core.Dto.CVDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Application.Mapper
{
    public class CVTemplateProfile : Profile
    {
        public CVTemplateProfile()
        {
            CreateMap<CV_Template,CVTemplateDto>().ReverseMap();
        }
    }
}