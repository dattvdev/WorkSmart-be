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
        public async Task SendMessage(PersonalMessage message)
        {
            // Send to the specific receiver
            await Clients.User(message.ReceiverID.ToString()).SendAsync("ReceiveMessage", message);
            // Also send to the sender to confirm message was sent
            await Clients.User(message.SenderID.ToString()).SendAsync("ReceiveMessage", message);
        }

        public async Task MarkAsRead(int senderId, int receiverId)
        {
            // Notify the sender that their messages have been read
            await Clients.User(senderId.ToString()).SendAsync("MessagesRead", receiverId);
        }

        public async Task JoinConversation(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public async Task LeaveConversation(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public override async Task OnConnectedAsync()
        {
            // You can add the user to their personal group when they connect
            var userId = Context.User.Identity.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }

            await base.OnConnectedAsync();
        }
    }
}
