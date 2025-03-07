using Microsoft.EntityFrameworkCore;
using WorkSmart.Core.Dto.MessageDtos;
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

        public async Task<IEnumerable<PersonalMessage>> GetConversationMessagesAsync(int senderId, int receiverId, int skip = 0, int take = 20)
        {
            return await _context.PersonalMessages
                .Where(m =>
                    (m.SenderID == senderId && m.ReceiverID == receiverId) ||
                    (m.SenderID == receiverId && m.ReceiverID == senderId))
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ConversationUserDto>> GetUserConversationsAsync(int userId)
        {
            // Lấy danh sách người dùng đang có hội thoại với userId
            var conversationPartnerIds = await _context.PersonalMessages
                .Where(m => m.SenderID == userId || m.ReceiverID == userId)
                .Select(m => m.SenderID == userId ? m.ReceiverID : m.SenderID)
                .Distinct()
                .ToListAsync();

            var result = new List<ConversationUserDto>();

            foreach (var partnerId in conversationPartnerIds)
            {
                // Lấy thông tin user
                var user = await _context.Users.FindAsync(partnerId);
                if (user == null) continue;

                // Đếm số tin nhắn chưa đọc
                var unreadCount = await _context.PersonalMessages
                    .CountAsync(m => m.SenderID == partnerId && m.ReceiverID == userId && !m.IsRead);

                // Lấy tin nhắn cuối cùng
                var lastMessage = await _context.PersonalMessages
                    .Where(m => (m.SenderID == userId && m.ReceiverID == partnerId) ||
                                (m.SenderID == partnerId && m.ReceiverID == userId))
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefaultAsync();

                // Thêm vào kết quả
                result.Add(new ConversationUserDto
                {
                    UserId = user.UserID,
                    FullName = user.FullName,
                    Avatar = user.Avatar,
                    LastMessageTime = lastMessage?.CreatedAt,
                    UnreadCount = unreadCount,
                    LastMessage = lastMessage?.Content
                });
            }

            // Sắp xếp theo thời gian tin nhắn cuối cùng
            return result.OrderByDescending(x => x.LastMessageTime);
        }

        public async Task<int> CountUnreadMessagesAsync(int receiverId)
        {
            return await _context.PersonalMessages
                .CountAsync(m => m.ReceiverID == receiverId && !m.IsRead);
        }

        public async Task MarkAllAsReadAsync(int senderId, int receiverId)
        {
            var unreadMessages = await _context.PersonalMessages
                .Where(m => m.SenderID == senderId && m.ReceiverID == receiverId && !m.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
