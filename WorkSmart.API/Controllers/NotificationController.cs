using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkSmart.API.Hubs;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.NotificationDtos;

namespace WorkSmart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : Controller
    {
        private readonly NotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;
        public NotificationController(NotificationService notificationService, IHubContext<NotificationHub> hubContext)
        {
            _notificationService = notificationService;
            _hubContext = hubContext;
        }
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            _notificationService.DeleteNotification(id);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<GetNotificationDto>>> GetNotificationsByUserId(int userId)
        {
            var notifications = await _notificationService.GetByUserId(userId);
            return Ok(notifications);
        }
        [HttpGet("unread/{userId}")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            var count = await _notificationService.GetUnreadNotificationsCount(userId);
            return Ok(count);
        }
        [HttpPut("markAsRead/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _notificationService.MarkNotificationAsRead(id);
            if (result)
            {
                return Ok(new { success = true });
            }
            return NotFound(new { success = false, message = "Notification not found" });
        }

    }
}
