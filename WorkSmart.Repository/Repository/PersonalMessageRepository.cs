using Microsoft.EntityFrameworkCore;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class PersonalMessageRepository : IPersonalMessageRepository
    {
        private readonly WorksmartDBContext _context;

        public PersonalMessageRepository(WorksmartDBContext context)
        {
            _context = context;
        }

        public async Task<PersonalMessage> AddAsync(PersonalMessage message)
        {
            await _context.PersonalMessages.AddAsync(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<PersonalMessage> UpdateAsync(PersonalMessage message)
        {
            _context.PersonalMessages.Update(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<PersonalMessage> GetByIdAsync(int id)
        {
            return await _context.PersonalMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m => m.PersonalMessageID == id);
        }

        public async Task<IEnumerable<PersonalMessage>> GetConversationMessagesAsync(int senderId, int receiverId)
        {
            return await _context.PersonalMessages
                .Where(m =>
                    (m.SenderID == senderId && m.ReceiverID == receiverId) ||
                    (m.SenderID == receiverId && m.ReceiverID == senderId))
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PersonalMessage>> GetUnreadMessagesAsync(int receiverId)
        {
            return await _context.PersonalMessages
                .Where(m => m.ReceiverID == receiverId && !m.IsRead)
                .Include(m => m.Sender)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CountUnreadMessagesAsync(int receiverId)
        {
            return await _context.PersonalMessages
                .CountAsync(m => m.ReceiverID == receiverId && !m.IsRead);
        }
    }
}
