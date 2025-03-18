using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using WorkSmart.API.Hubs;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.MessageDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MessageService _messageService;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagesController(MessageService messageService, IHubContext<ChatHub> hubContext)
        {
            _messageService = messageService;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> SendMessage(SendMessageDto messageDto)
        {
            var message = await _messageService.SendMessageAsync(messageDto);

            // Notify receiver about new message through SignalR
            await _hubContext.Clients.Group($"user_{messageDto.ReceiverID}").SendAsync("ReceiveMessage", message);
            await _hubContext.Clients.Group($"user_{messageDto.SenderID}").SendAsync("ReceiveMessage", message);
            // Update unread message count for receiver
            var unreadCount = await _messageService.GetUnreadMessageCountAsync(messageDto.ReceiverID);
            await _hubContext.Clients.Group($"user_{messageDto.ReceiverID}").SendAsync("UpdateUnreadCount", unreadCount);

            // Update conversation lists for both users
            await UpdateConversationList(messageDto.SenderID);
            await UpdateConversationList(messageDto.ReceiverID);

            return Ok(message);
        }

        [HttpGet("{userId1}/{userId2}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesBetweenUsers(
            int userId1,
            int userId2,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var messages = await _messageService.GetMessagesBetweenUsersAsync(userId1, userId2, pageNumber, pageSize);
            return Ok(messages);
        }

        [HttpGet("conversations/{userId}")]
        public async Task<ActionResult<IEnumerable<ConversationDto>>> GetConversations(int userId)
        {
            var conversations = await _messageService.GetConversationsAsync(userId);
            return Ok(conversations);
        }

        [HttpGet("unread/{userId}")]
        public async Task<ActionResult<UnreadCountDto>> GetUnreadMessageCount(int userId)
        {
            var count = await _messageService.GetUnreadMessageCountAsync(userId);
            return Ok(new UnreadCountDto { Count = count });
        }

        [HttpPut("read/{senderId}/{receiverId}")]
        public async Task<ActionResult> MarkMessagesAsRead(int senderId, int receiverId)
        {
            await _messageService.MarkMessagesAsReadAsync(senderId, receiverId);

            // Update unread count after marking messages as read
            var unreadCount = await _messageService.GetUnreadMessageCountAsync(receiverId);
            await _hubContext.Clients.Group($"user_{receiverId}").SendAsync("UpdateUnreadCount", unreadCount);

            // Update conversation list for receiver
            await UpdateConversationList(receiverId);

            return NoContent();
        }

        [HttpPut("read-all/{userId}")]
        public async Task<ActionResult> MarkAllMessagesAsRead(int userId)
        {
            await _messageService.MarkAllMessagesAsReadAsync(userId);

            // Update unread count after marking all messages as read
            await _hubContext.Clients.Group($"user_{userId}").SendAsync("UpdateUnreadCount", 0);

            // Update conversation list
            await UpdateConversationList(userId);

            return NoContent();
        }

        // Helper method to update conversation list via SignalR
        private async Task UpdateConversationList(int userId)
        {
            var conversations = await _messageService.GetConversationsAsync(userId);
            await _hubContext.Clients.Group($"user_{userId}").SendAsync("UpdateConversations", conversations);
        }
    }
}
