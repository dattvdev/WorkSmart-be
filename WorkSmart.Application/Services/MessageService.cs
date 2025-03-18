using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
        private readonly IMapper _mapper;
        public MessageService(IPersonalMessageRepository messageRepository, IMapper mapper)
        {
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        public async Task<MessageDto> SendMessageAsync(SendMessageDto messageDto)
        {
            /*var message = new PersonalMessage
            {
                SenderID = messageDto.SenderId,
                ReceiverID = messageDto.ReceiverId,
                Content = messageDto.Content,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
*/
            PersonalMessage personalMessage = _mapper.Map<PersonalMessage>(messageDto);

            await _messageRepository.Add(personalMessage);

            // Get sender and receiver for full details
            var sender = await _messageRepository.GetById(personalMessage.SenderID);
            var receiver = await _messageRepository.GetById(personalMessage.ReceiverID);

           /* return new MessageDto
            {
                MessageId = message.PersonalMessageID,
                SenderId = message.SenderID,
                SenderName = sender?.FullName,
                SenderAvatar = sender?.Avatar,
                ReceiverId = message.ReceiverID,
                ReceiverName = receiver?.FullName,
                ReceiverAvatar = receiver?.Avatar,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                IsRead = message.IsRead
            };*/

            return _mapper.Map<MessageDto>(personalMessage);
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesBetweenUsersAsync(int userId1, int userId2, int pageNumber = 1, int pageSize = 20)
        {
            var messages = await _messageRepository.GetMessagesBetweenUsersAsync(userId1, userId2, pageNumber, pageSize);

            /*return messages.Select(m => new MessageDto
            {
                MessageId = m.PersonalMessageID,
                SenderId = m.SenderID,
                SenderName = m.Sender?.FullName,
                SenderAvatar = m.Sender?.Avatar,
                ReceiverId = m.ReceiverID,
                ReceiverName = m.Receiver?.FullName,
                ReceiverAvatar = m.Receiver?.Avatar,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                IsRead = m.IsRead
            });*/
            return _mapper.Map<IEnumerable<MessageDto>>(messages).OrderBy(pm => pm.CreatedAt);
        }

        public async Task<int> GetUnreadMessageCountAsync(int userId)
        {
            return await _messageRepository.GetUnreadMessageCountAsync(userId);
        }

        public async Task MarkMessagesAsReadAsync(int senderId, int receiverId)
        {
            await _messageRepository.MarkMessagesAsReadAsync(senderId, receiverId);
        }

        public async Task MarkAllMessagesAsReadAsync(int receiverId)
        {
            await _messageRepository.MarkAllMessagesAsReadAsync(receiverId);
        }

        public async Task<IEnumerable<ConversationDto>> GetConversationsAsync(int userId)
        {
            // Get all users who have exchanged messages with the current user
            var conversationUsers = await _messageRepository.GetConversationUsersAsync(userId);
            // Sort conversations by most recent message
            return conversationUsers.OrderByDescending(c => c.LastMessageTime);
        }
    }   
}
