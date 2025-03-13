using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.MessageDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.API.Hubs
{
    public class ChatHub : Hub
    {
        public static readonly ConcurrentDictionary<int, string> UserConnections = new ConcurrentDictionary<int, string>();

        public async Task ConnectUser(int userId)
        {
            UserConnections[userId] = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            await Clients.Others.SendAsync("UserConnected", userId);
        }

        public async Task SendMessage(int receiverId, string message)
        {
            if (UserConnections.TryGetValue(receiverId, out string connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
            }
        }

        public async Task MarkAsRead(int messageId, int senderId)
        {
            if (UserConnections.TryGetValue(senderId, out string connectionId))
            {
                await Clients.Client(connectionId).SendAsync("MessageRead", messageId);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            int userId = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (userId != 0)
            {
                UserConnections.TryRemove(userId, out _);
                await Clients.Others.SendAsync("UserDisconnected", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
