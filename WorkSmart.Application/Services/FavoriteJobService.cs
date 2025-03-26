using AutoMapper;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class FavoriteJobService
    {
        private readonly IFavoriteJobRepository _favoriteJobRepository;
        private readonly IMapper _mapper;

        public FavoriteJobService(IFavoriteJobRepository favoriteJobRepository
            ,IMapper mapper)
        {
            _favoriteJobRepository = favoriteJobRepository;
            _mapper = mapper;
        }

        public 
    }
}
