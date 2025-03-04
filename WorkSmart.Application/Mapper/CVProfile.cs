using AutoMapper;
using WorkSmart.Core.Dto.CVDtos;
using WorkSmart.Core.Entity;

public class CVProfile : Profile
{
    public CVProfile()
    {
        CreateMap<CV,CVDto>().ReverseMap();
        CreateMap<CV_Certification,CVCertificationDto>().ReverseMap();
        CreateMap<CV_Experience,CVExperienceDto>().ReverseMap();
        CreateMap<CV_Skill,CVSkillDto>().ReverseMap();
        CreateMap<CV_Education,CVEducationDto>().ReverseMap();
    }
}
