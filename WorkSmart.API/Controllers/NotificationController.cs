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
        public async Task<IActionResult> Delete(int id)
        {
            // Get notification details before deletion to know which user to notify
            var notification = await _notificationService.GetNotificationById(id);
            if (notification == null)
            {
                return NotFound(new { success = false, message = "Notification not found" });
            }

            _notificationService.DeleteNotification(id);

            // Broadcast deletion to all of the user's clients
            await _hubContext.Clients.Group(notification.UserID.ToString())
                .SendAsync("NotificationDeleted", id);

            // Update unread count
            var unreadCount = await _notificationService.GetUnreadNotificationsCount(notification.UserID);
            await _hubContext.Clients.Group(notification.UserID.ToString())
                .SendAsync("UnreadCountUpdated", unreadCount);

            return Ok(new { success = true });
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
                // Get the notification to find the userId
                var notification = await _notificationService.GetNotificationById(id);
                if (notification != null)
                {
                    // Notify all clients in the user's group that this notification was read
                    await _hubContext.Clients.Group(notification.UserID.ToString())
                        .SendAsync("NotificationRead", id);

                    // Also broadcast the new unread count
                    var unreadCount = await _notificationService.GetUnreadNotificationsCount(notification.UserID);
                    await _hubContext.Clients.Group(notification.UserID.ToString())
                        .SendAsync("UnreadCountUpdated", unreadCount);
                }

                return Ok(new { success = true });
            }
            return NotFound(new { success = false, message = "Notification not found" });
        }

        [HttpGet("candidate-job/{userId}")]
        public async Task<IActionResult> GetCandidateJobNotifications(int userId)
        {
            var notifications = await _notificationService.GetCandidateJobNotifications(userId);
            return Ok(notifications);
        }
    }
}
