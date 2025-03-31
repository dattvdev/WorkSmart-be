using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.FavoriteJobDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.API.Controllers
{
    [ApiController]
    [Route("favoritejob")]
    public class FavoriteJobController : ControllerBase
    {
        private readonly FavoriteJobService _favoriteJobService;
        private readonly IMapper _mapper;

        public FavoriteJobController(FavoriteJobService favoriteJobService,IMapper mapper)
        {
            _favoriteJobService = favoriteJobService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FavoriteJobDto>>> GetAll()
        {
            var favoriteJobs = await _favoriteJobService.GetAllAsync();
            var favoriteJobDtos = _mapper.Map<IEnumerable<FavoriteJobDto>>(favoriteJobs);
            return Ok(favoriteJobDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FavoriteJobDto>> GetById(int id)
        {
            var favoriteJob = await _favoriteJobService.GetByIdAsync(id);
            if (favoriteJob == null)
                return NotFound();

            var favoriteJobDto = _mapper.Map<FavoriteJobDto>(favoriteJob);
            return Ok(favoriteJobDto);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<FavoriteJobDto>>> GetByUserId(int userId)
        {
            var favoriteJobs = await _favoriteJobService.GetByUserIdAsync(userId);
            var favoriteJobDtos = _mapper.Map<IEnumerable<FavoriteJobDto>>(favoriteJobs);
            return Ok(favoriteJobDtos);
        }

        [HttpPost]
        public async Task<ActionResult<FavoriteJobDto>> Add([FromBody] CreateFavoriteJobDto dto)
        {
            var createdFavoriteJob = await _favoriteJobService.AddAsync(dto);
            return CreatedAtAction(nameof(GetById),new { id = createdFavoriteJob.FavoriteJobID },createdFavoriteJob);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _favoriteJobService.DeleteAsync(id);
            return NoContent();
        }
    }
}