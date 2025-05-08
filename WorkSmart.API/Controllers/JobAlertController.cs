using Microsoft.AspNetCore.Mvc;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobAlertController : ControllerBase
    {
        private readonly JobAlertService _jobAlertService;

        public JobAlertController(JobAlertService jobAlertService)
        {
            _jobAlertService = jobAlertService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllJobAlert()
        {
            try
            {
                var jobAlerts = await _jobAlertService.GetAllJobAlertsAsync();
                return Ok(jobAlerts);
            }
            catch (Exception ex)
            {
                return StatusCode(500,new { message = "An error occurred while retrieving job alerts." });
            }
        }


        // POST: api/JobAlert
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JobAlertCreateDto dto)
        {
            var result = await _jobAlertService.CreateJobAlert(dto);
            if (result)
                return Ok(new { message = "Create job alert successfully" });
            return BadRequest(new { message = "Failed to create job alert" });
        }

        // GET: api/JobAlert/user/5
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var result = await _jobAlertService.GetAlertsByUser(userId);
            return Ok(result);
        }

        // DELETE: api/JobAlert/5/user/1
        [HttpDelete("{alertId}/user/{userId}")]
        public async Task<IActionResult> Delete(int alertId, int userId)
        {
            var result = await _jobAlertService.DeleteAlert(alertId, userId);
            if (result)
                return Ok(new { message = "Xóa cảnh báo thành công" });
            return NotFound(new { message = "Không tìm thấy hoặc không có quyền xóa" });
        }

        [HttpGet("alerts/{jobId}")]
        public async Task<IActionResult> GetJobAlertsByJobId(int jobId)
        {
            var alerts = await _jobAlertService.GetJobAlertsByJobId(jobId);
            if (alerts == null || !alerts.Any())
            {
                return NotFound(new { message = "No job alerts found for this job." });
            }

            return Ok(alerts);
        }

    }
}

