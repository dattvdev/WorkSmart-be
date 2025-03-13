using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.MessageDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface IPersonalMessageRepository
    {
        Task<PersonalMessage> AddAsync(PersonalMessage message);
        Task<PersonalMessage> UpdateAsync(PersonalMessage message);
        Task<PersonalMessage> GetByIdAsync(int id);
        Task<IEnumerable<PersonalMessage>> GetConversationMessagesAsync(int senderId, int receiverId, int skip = 0, int take = 20);
        Task<IEnumerable<ConversationUserDto>> GetUserConversationsAsync(int userId);
        Task<int> CountUnreadMessagesAsync(int receiverId);
        Task MarkAllAsReadAsync(int senderId, int receiverId);
    }
}
