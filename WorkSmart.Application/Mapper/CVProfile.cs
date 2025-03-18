using AutoMapper;
using WorkSmart.Core.Dto.CVDtos;
using WorkSmart.Core.Entity;

public class CVProfile : Profile
{
    public CVProfile()
    {
        CreateMap<CV,CVDto>().ReverseMap();

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

        CreateMap<CV_Skill,CVSkillDto>().ReverseMap();

        CreateMap<CV_Education, CVEducationDto>()
                .ForMember(dest => dest.SchoolName, opt => opt.MapFrom(src => src.SchoolName))
                .ForMember(dest => dest.Degree, opt => opt.MapFrom(src => src.Degree))
                .ForMember(dest => dest.StartedAt, opt => opt.MapFrom(src => src.StartedAt))
                .ForMember(dest => dest.EndedAt, opt => opt.MapFrom(src => src.EndedAt))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ReverseMap();

        CreateMap<CV,CvUploadDto>().ReverseMap();
    }
}
