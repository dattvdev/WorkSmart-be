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
        private static Dictionary<int, string> _userConnections = new Dictionary<int, string>();

        public async Task SendPrivateMessage(MessageDto message)
        {
            // Send to specific user if they're connected
            if (_userConnections.TryGetValue(message.ReceiverId, out string connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
            }

            // Also send back to sender to update their UI
            await Clients.Caller.SendAsync("ReceiveMessage", message);
        }

        public async Task NotifyUnreadCount(int userId, int count)
        {
            if (_userConnections.TryGetValue(userId, out string connectionId))
            {
                await Clients.Client(connectionId).SendAsync("UpdateUnreadCount", count);
            }
        }

        public async Task UpdateConversationList(int userId, IEnumerable<ConversationDto> conversations)
        {
            if (_userConnections.TryGetValue(userId, out string connectionId))
            {
                await Clients.Client(connectionId).SendAsync("UpdateConversations", conversations);
            }
        }

        public async Task RegisterUser(int userId)
        {
            // Remove user from any previous connection
            var oldConnection = _userConnections.FirstOrDefault(x => x.Key == userId);
            if (oldConnection.Key != 0)
            {
                _userConnections.Remove(oldConnection.Key);
            }

            // Register new connection
            _userConnections[userId] = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            // Notify others this user is online
            await Clients.Others.SendAsync("UserOnline", userId);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = _userConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (userId != 0)
            {
                _userConnections.Remove(userId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");

                // Notify others this user is offline
                await Clients.Others.SendAsync("UserOffline", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
