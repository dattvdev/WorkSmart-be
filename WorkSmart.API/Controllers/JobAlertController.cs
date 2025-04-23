using Microsoft.AspNetCore.Mvc;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.JobDtos;

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

        // POST: api/JobAlert
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JobAlertCreateDto dto)
        {
            var result = await _jobAlertService.CreateJobAlert(dto);
            if (result)
                return Ok(new { message = "Tạo cảnh báo công việc thành công" });
            return BadRequest(new { message = "Tạo thất bại" });
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
        public async Task<IActionResult> Delete(int alertId,int userId)
        {
            var result = await _jobAlertService.DeleteAlert(alertId,userId);
            if (result)
                return Ok(new { message = "Xóa cảnh báo thành công" });
            return NotFound(new { message = "Không tìm thấy hoặc không có quyền xóa" });
        }
    }
}

