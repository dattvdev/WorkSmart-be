using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface IPersonalMessageRepository
    {
        Task<PersonalMessage> AddAsync(PersonalMessage message);
        Task<PersonalMessage> UpdateAsync(PersonalMessage message);
        Task<PersonalMessage> GetByIdAsync(int id);
        Task<IEnumerable<PersonalMessage>> GetConversationMessagesAsync(int senderId, int receiverId);
        Task<IEnumerable<PersonalMessage>> GetUnreadMessagesAsync(int receiverId);
        Task<int> CountUnreadMessagesAsync(int receiverId);
    }
}
