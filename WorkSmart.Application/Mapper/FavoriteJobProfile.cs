using AutoMapper;
using WorkSmart.Core.Dto.FavoriteJobDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Application.Mapper
{
    public class FavoriteJobProfile : Profile
    {
        public FavoriteJobProfile()
        {
            CreateMap<FavoriteJob,FavoriteJobDto>();
            CreateMap<CreateFavoriteJobDto,FavoriteJob>();
        }
        
    }
}
