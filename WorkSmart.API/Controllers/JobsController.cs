using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto;
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

        public JobController(JobService jobService)
        {
            _jobService = jobService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto createJobDto)
        {
            var job = new Job
            {
                UserID = createJobDto.EmployerID,
                Title = createJobDto.Title,
                Description = createJobDto.Description,
                Location = createJobDto.Location,
                Salary = (double?)createJobDto.Salary,
                Status = JobStatus.Pending,
                CreatedAt = DateTime.Now
            };

            var newJob = await _jobService.CreateJobAsync(job);
            return Ok(new JobDto
            {
                JobID = newJob.JobID,
                Title = newJob.Title,
                Description = newJob.Description,
                Location = newJob.Location,
                Salary = newJob.Salary,
                Status = newJob.Status.ToString(),
                CreatedAt = newJob.CreatedAt
            });
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] UpdateJobDto updateJobDto)
        {
            var existingJob = await _jobService.GetJobByIdAsync(id);
            if (existingJob == null) return NotFound("Job not found");

            existingJob.Title = updateJobDto.Title;
            existingJob.Description = updateJobDto.Description;
            existingJob.Location = updateJobDto.Location;
            existingJob.Salary = (double?)updateJobDto.Salary;
            existingJob.Status = updateJobDto.Status;

            var updatedJob = await _jobService.UpdateJobAsync(existingJob);
            return Ok(new JobDto
            {
                JobID = updatedJob.JobID,
                Title = updatedJob.Title,
                Description = updatedJob.Description,
                Location = updatedJob.Location,
                Salary = updatedJob.Salary,
                Status = updatedJob.Status.ToString(),
                CreatedAt = updatedJob.CreatedAt
            });
        }
    }
}
