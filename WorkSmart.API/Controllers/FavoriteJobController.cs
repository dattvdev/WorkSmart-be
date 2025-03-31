using Microsoft.AspNetCore.Mvc;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.FavoriteJobDtos;

namespace WorkSmart.API.Controllers
{
    [ApiController]
    [Route("favoritejob")]
    public class FavoriteJobController : ControllerBase
    {
        private readonly FavoriteJobService _favoriteJobService;

        public FavoriteJobController(FavoriteJobService favoriteJobService)
        {
            _favoriteJobService = favoriteJobService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FavoriteJobDto>>> GetAll()
        {
            var favoriteJobs = await _favoriteJobService.GetAllAsync();
            return Ok(favoriteJobs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FavoriteJobDto>> GetById(int id)
        {
            var favoriteJob = await _favoriteJobService.GetByIdAsync(id);
            if (favoriteJob == null)
                return NotFound();
            return Ok(favoriteJob);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<FavoriteJobDto>>> GetByUserId(int userId)
        {
            var favoriteJobs = await _favoriteJobService.GetByUserIdAsync(userId);
            return Ok(favoriteJobs);
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
