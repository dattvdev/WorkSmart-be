using Microsoft.AspNetCore.Mvc;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.JobDtos;

namespace WorkSmart.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobRecommendationController : ControllerBase
    {
        private readonly JobRecommendationService _service;

        public JobRecommendationController(JobRecommendationService service)
        {
            _service = service;
        }

        [HttpGet("cv/{cvId}")]
        [ProducesResponseType(typeof(List<JobRecommendationDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Recommend(int cvId)
        {
            var result = await _service.GetRecommendedJobsForCV(cvId);
            return Ok(result);
        }

        [HttpPost("clear-cache/{cvId}")]
        public IActionResult ClearCache(int cvId)
        {
            _service.ClearCVRecommendationCache(cvId);
            return Ok("Cache cleared");
        }

        [HttpPost("update-job-embedding/{jobId}")]
        public async Task<IActionResult> UpdateJobVector(int jobId)
        {
            var job = await _service.GetJobById(jobId);
            if (job == null) return NotFound();
            await _service.UpdateJobEmbedding(job);
            return Ok("Embedding updated");
        }

        [HttpDelete("delete-cv-vector/{cvId}")]
        public async Task<IActionResult> DeleteCVVector(int cvId)
        {
            await _service.DeleteCVEmbedding(cvId);
            return Ok("CV vector deleted");
        }

        [HttpDelete("delete-job-vector/{jobId}")]
        public async Task<IActionResult> DeleteJobVector(int jobId)
        {
            await _service.DeleteJobEmbedding(jobId);
            return Ok("Job vector deleted");
        }
    }
}
