using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;

namespace WorkSmart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly JobService _jobService;
        private readonly ILogger<JobController> _logger;

        public JobController(JobService jobService, ILogger<JobController> logger)
        {
            _jobService = jobService;
            _logger = logger;
        }

        /// Create a new job post
        
        [HttpPost("create")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto createJobDto)
        {
            if (createJobDto == null)
                return BadRequest(new { message = "Invalid job data." });
            try
            {
                await _jobService.CreateJobAsync(createJobDto);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating job: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while creating the job." });
            }
        }
        [HttpGet("getAllJob")]
        public async Task<IActionResult> GetAllJob()
        {
            try
            {
                var jobs = await _jobService.GetAllJobsAsync();
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error lits job: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while list the job." });

            }
        }
        [HttpDelete("delete/{jobId}")]
        public IActionResult DeleteJob(int jobId)
        {
            try
            {
                _jobService.DeleteJob(jobId);
                return Ok(new { message = "Job deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to delete job", error = ex.Message });
            }
        }


        /// Update an existing job post
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] UpdateJobDto updateJobDto)
        {
            if (updateJobDto == null)
                return BadRequest(new { message = "Invalid job data." });

            try
            {
                var updatedJob = await _jobService.UpdateJobAsync(id, updateJobDto);
                if (updatedJob == null)
                    return NotFound(new { message = "Job not found." });

                return Ok(updatedJob);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating job ID {JobID}: {Message}", id, ex.Message);
                return StatusCode(500, new { message = "An error occurred while updating the job." });
            }
        }

        /// Hide a job post
        [HttpPut("hide/{id}")]
        public async Task<IActionResult> HideJob(int id)
        {
            try
            {
                var success = await _jobService.HideJobAsync(id);
                if (!success)
                    return NotFound(new { message = "Job not found." });

                return Ok(new { message = "Job post has been hidden successfully.", jobId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error hiding job ID {JobID}: {Message}", id, ex.Message);
                return StatusCode(500, new { message = "An error occurred while hiding the job." });
            }
        }

        /// Unhide a job post
        [HttpPut("unhide/{id}")]
        public async Task<IActionResult> UnhideJob(int id)
        {
            try
            {
                var success = await _jobService.UnhideJobAsync(id);
                if (!success)
                    return NotFound(new { message = "Job not found." });

                return Ok(new { message = "Job post has been unhidden successfully.", jobId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error unhiding job ID {JobID}: {Message}", id, ex.Message);
                return StatusCode(500, new { message = "An error occurred while unhiding the job." });
            }
        }

        /// Get job details by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobById(int id)
        {
            try
            {
                var job = await _jobService.GetJobById(id);
                if (job == null)
                    return NotFound(new { message = "Job not found." });

                return Ok(job);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching job ID {JobID}: {Message}", id, ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving the job." });
            }
        }

        [HttpGet("GetListSearch")]
        public async Task<IActionResult> GetListSearch
            ([FromQuery] JobSearchRequestDto request)
        {
            var (jobs, total) = await _jobService.GetListSearch(request);
            var totalPage = (int)Math.Ceiling((double)total / request.PageSize);
            var totalJob = total;
            return Ok(new {totalJob, totalPage, jobs });
        }

        ///// Get jobs by employer ID
        //[HttpGet("employer/{employerId}")]
        //public async Task<IActionResult> GetJobsByEmployerId(int employerId)
        //{
        //    try
        //    {
        //        var jobs = await _jobService.GetJobsByEmployerId(employerId);
        //        return Ok(jobs);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Error fetching jobs for employer ID {EmployerID}: {Message}", employerId, ex.Message);
        //        return StatusCode(500, new { message = "An error occurred while retrieving jobs." });
        //    }
        //}

        ///// Get jobs by status
        //[HttpGet("status/{status}")]
        //public async Task<IActionResult> GetJobsByStatus(JobStatus status)
        //{
        //    try
        //    {
        //        var jobs = await _jobService.GetJobsByStatus(status);
        //        return Ok(jobs);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Error fetching jobs with status {Status}: {Message}", status, ex.Message);
        //        return StatusCode(500, new { message = "An error occurred while retrieving jobs." });
        //    }
        //}
        [HttpPost("hide-expired")]
        public async Task<IActionResult> HideExpiredJobs()
        {
            try
            {
                var result = await _jobService.HideExpiredJobsAsync();
                return Ok(new
                {
                    Success = true,
                    Message = $"Đã ẩn {result.HiddenCount} job hết hạn",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error hiding expired jobs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi ẩn job hết hạn."
                });
            }
        }
    }
}
