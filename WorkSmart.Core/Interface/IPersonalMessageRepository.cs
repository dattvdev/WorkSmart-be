using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.MessageDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface IPersonalMessageRepository : IBaseRepository<PersonalMessage>
    {
        Task<IEnumerable<PersonalMessage>> GetMessagesBetweenUsersAsync(int userId1, int userId2, int pageNumber, int pageSize);
        Task<int> GetUnreadMessageCountAsync(int receiverId);
        Task<int> GetUnreadMessageCountFromSenderAsync(int senderId, int receiverId);
        Task MarkMessagesAsReadAsync(int senderId, int receiverId);
        Task MarkAllMessagesAsReadAsync(int receiverId);
        Task<IEnumerable<ConversationDto>> GetConversationUsersAsync(int userId);
        Task<IEnumerable<PersonalMessage>> FindAsync(Expression<Func<PersonalMessage, bool>> predicate);
    }
}
