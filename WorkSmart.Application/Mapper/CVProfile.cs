using AutoMapper;
using WorkSmart.Core.Dto.CVDtos;
using WorkSmart.Core.Entity;

public class CVProfile : Profile
{
    public CVProfile()
    {
        CreateMap<CreateCVDto,CV>();
        CreateMap<CreateCVEducationDto,CV_Education>();
        CreateMap<CreateCVExperienceDto,CV_Experience>();
        CreateMap<CreateCVSkillDto,CV_Skill>();
    }
}
