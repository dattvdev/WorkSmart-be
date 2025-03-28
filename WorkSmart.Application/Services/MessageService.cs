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
            PersonalMessage personalMessage = _mapper.Map<PersonalMessage>(messageDto);

            await _messageRepository.Add(personalMessage);

            return _mapper.Map<MessageDto>(personalMessage);
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesBetweenUsersAsync(int userId1, int userId2, int pageNumber = 1, int pageSize = 20)
        {
            var messages = await _messageRepository.GetMessagesBetweenUsersAsync(userId1, userId2, pageNumber, pageSize);

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
