using AutoMapper;
using WorkSmart.Core.Dto.CVDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Application.Mapper
{
    public class CVProfile : Profile
    {
        public CVProfile()
        {
            CreateMap<CV,CVDto>().ReverseMap();
            CreateMap<CreateCVDto,CV>();
        }
    }

}
