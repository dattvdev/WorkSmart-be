using Microsoft.AspNetCore.SignalR;
using WorkSmart.API.Hubs;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.NotificationDtos;

namespace WorkSmart.API.SignalRService
{
    public class SignalRNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly NotificationService _notificationService;

        public SignalRNotificationService(
            IHubContext<NotificationHub> hubContext,
            NotificationService notificationService)
        {
            _hubContext = hubContext;
            _notificationService = notificationService;
        }

        public async Task SendNotificationToUser(int userId, string title, string message, string link = null)
        {
            // Lưu thông báo vào database
            var notificationDto = new GetNotificationDto
            {
                UserID = userId,
                Title = title,
                Message = message,
                Link = link,
                IsRead = false
            };

            await _notificationService.CreateNotification(notificationDto);

            // Gửi realtime notification qua SignalR
            await _hubContext.Clients.Group(userId.ToString())
                .SendAsync("ReceiveNotification", title, message, link);
        }
    }
}
