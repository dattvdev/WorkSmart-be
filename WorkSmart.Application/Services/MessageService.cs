using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.MessageDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class MessageService
    {
        private readonly IPersonalMessageRepository _messageRepository;

        public MessageService(IPersonalMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task<PersonalMessage> SendMessageAsync(PersonalMessage message)
        {
            // Bổ sung logic validation nếu cần

            // Lưu message vào database
            return await _messageRepository.AddAsync(message);
        }

        public async Task<PersonalMessage> MarkMessageAsReadAsync(int messageId)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message != null)
            {
                message.IsRead = true;
                await _messageRepository.UpdateAsync(message);
            }
            return message;
        }

        public async Task MarkAllMessagesAsReadAsync(int senderId, int receiverId)
        {
            await _messageRepository.MarkAllAsReadAsync(senderId, receiverId);
        }

        public async Task<IEnumerable<PersonalMessage>> GetConversationMessagesAsync(int senderId, int receiverId, int page = 0, int pageSize = 20)
        {
            int skip = page * pageSize;
            return await _messageRepository.GetConversationMessagesAsync(senderId, receiverId, skip, pageSize);
        }

        public async Task<IEnumerable<ConversationUserDto>> GetUserConversationsAsync(int userId)
        {
            return await _messageRepository.GetUserConversationsAsync(userId);
        }

        public async Task<int> GetUnreadMessageCountAsync(int userId)
        {
            return await _messageRepository.CountUnreadMessagesAsync(userId);
        }
    }   
}
