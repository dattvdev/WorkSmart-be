using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSmart.Application.Services;

namespace WorkSmart.API.Controllers
{
    [Route("NotificationSetting")]
    [ApiController]
    public class NotificationSettingController : Controller
    {
        private readonly NotificationSettingService _notificationSettingService;
        public NotificationSettingController(NotificationSettingService notificationSettingService)
        {
            _notificationSettingService = notificationSettingService;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userRole = User.FindFirst("Role")?.Value;
            var result = await _notificationSettingService.GetByIdAsync(id, userRole);
            if (result == null)
            {
                return NotFound(new { message = "Notification setting not found" });
            }
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] object dto)
        {
            var userRole = User.FindFirst("Role")?.Value;
            var result = await _notificationSettingService.UpdateAsync(id, dto, userRole);
            if (result)
            {
                return Ok(new { message = "Notification setting updated successfully" });
            }
            return NotFound(new { message = "Notification setting not found" });
        }
    }
}
