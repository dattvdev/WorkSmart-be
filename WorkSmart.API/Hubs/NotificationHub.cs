using Microsoft.AspNetCore.SignalR;
using WorkSmart.Core.Dto.NotificationDtos;

namespace WorkSmart.API.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string userId, string message, string title, string link)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message, title, link);
        }

        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}
