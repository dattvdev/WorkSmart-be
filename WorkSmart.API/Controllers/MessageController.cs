using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.MessageDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly MessageService _messageService;

        public MessageController(MessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage(SendMessageDto dto)
        {
            // Get current user ID from claims
            int currentUserId = int.Parse(User.FindFirst("UserID")?.Value);

            // Ensure the sender is the current user
            if (currentUserId != dto.SenderID)
            {
                return BadRequest("Sender ID must match authenticated user");
            }

            var message = new PersonalMessage
            {
                SenderID = dto.SenderID,
                ReceiverID = dto.ReceiverID,
                Content = dto.Content,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            var result = await _messageService.SendMessageAsync(message);
            return Ok(result);
        }

        [HttpGet("conversation/{receiverId}")]
        public async Task<IActionResult> GetConversation(int receiverId, [FromQuery] int page = 0, [FromQuery] int pageSize = 20)
        {
            int currentUserId = int.Parse(User.FindFirst("UserID")?.Value);
            var messages = await _messageService.GetConversationMessagesAsync(currentUserId, receiverId, page, pageSize);
            return Ok(messages);
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetUserConversations()
        {
            int currentUserId = int.Parse(User.FindFirst("UserID")?.Value);
            var conversations = await _messageService.GetUserConversationsAsync(currentUserId);
            return Ok(conversations);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            int currentUserId = int.Parse(User.FindFirst("UserID")?.Value);
            var count = await _messageService.GetUnreadMessageCountAsync(currentUserId);
            return Ok(new { count });
        }

        [HttpPost("mark-read/{messageId}")]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            var message = await _messageService.MarkMessageAsReadAsync(messageId);
            if (message == null)
            {
                return NotFound();
            }
            return Ok(message);
        }

        [HttpPost("mark-all-read/{senderId}")]
        public async Task<IActionResult> MarkAllAsRead(int senderId)
        {
            int currentUserId = int.Parse(User.FindFirst("UserID")?.Value);
            await _messageService.MarkAllMessagesAsReadAsync(senderId, currentUserId);
            return Ok();
        }
    }
}
