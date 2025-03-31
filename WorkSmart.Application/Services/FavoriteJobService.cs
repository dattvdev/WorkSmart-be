using AutoMapper;
using WorkSmart.Core.Dto.FavoriteJobDtos;
using WorkSmart.Core.Entity;
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
        public async Task<IEnumerable<FavoriteJobDto>> GetAllAsync()
        {
            var favoriteJobs = await _favoriteJobRepository.GetAll();
            return _mapper.Map<IEnumerable<FavoriteJobDto>>(favoriteJobs);
        }

        public async Task<FavoriteJobDto> GetByIdAsync(int id)
        {
            var favoriteJob = await _favoriteJobRepository.GetById(id);
            return _mapper.Map<FavoriteJobDto>(favoriteJob);
        }

        public async Task<IEnumerable<FavoriteJobDto>> GetByUserIdAsync(int userId)
        {
            var favoriteJobs = await _favoriteJobRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<FavoriteJobDto>>(favoriteJobs);
        }

        public async Task<FavoriteJobDto> AddAsync(CreateFavoriteJobDto dto)
        {
            var favoriteJob = _mapper.Map<FavoriteJob>(dto);
            await _favoriteJobRepository.Add(favoriteJob);
            return _mapper.Map<FavoriteJobDto>(favoriteJob);
        }

        public async Task DeleteAsync(int id)
        {
            _favoriteJobRepository.Delete(id);
        }
    }
}
