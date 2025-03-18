using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WorkSmart.Core.Dto.MessageDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class PersonalMessageRepository : BaseRepository<PersonalMessage>, IPersonalMessageRepository
    {
        private readonly WorksmartDBContext _context;

        public PersonalMessageRepository(WorksmartDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PersonalMessage>> FindAsync(Expression<Func<PersonalMessage, bool>> predicate)
        {
            return await _context.PersonalMessages
                .Include(pm => pm.Sender)
                .Include(pm => pm.Receiver)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PersonalMessage>> GetMessagesBetweenUsersAsync(int userId1, int userId2, int pageNumber, int pageSize)
        {
            var mess = await _context.PersonalMessages
                .Include(pm => pm.Sender)
                .Include(pm => pm.Receiver)
                .Where(pm =>
                    (pm.SenderID == userId1 && pm.ReceiverID == userId2) ||
                    (pm.SenderID == userId2 && pm.ReceiverID == userId1))
                .OrderByDescending(pm => pm.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            Console.WriteLine(mess);
            if (mess != null)
            {
                return mess;
            }
            return new List<PersonalMessage>();
        }

        public async Task<int> GetUnreadMessageCountAsync(int receiverId)
        {
            var count = await _context.PersonalMessages
                .CountAsync(pm => pm.ReceiverID == receiverId && !pm.IsRead);
            return count;
        }

        public async Task<int> GetUnreadMessageCountFromSenderAsync(int senderId, int receiverId)
        {
            return await _context.PersonalMessages
                .CountAsync(pm => pm.SenderID == senderId && pm.ReceiverID == receiverId && !pm.IsRead);
        }

        public async Task MarkMessagesAsReadAsync(int senderId, int receiverId)
        {
            var unreadMessages = await _context.PersonalMessages
                .Where(pm => pm.SenderID == senderId && pm.ReceiverID == receiverId && !pm.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task MarkAllMessagesAsReadAsync(int receiverId)
        {
            var unreadMessages = await _context.PersonalMessages
                .Where(pm => pm.ReceiverID == receiverId && !pm.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ConversationDto>> GetConversationUsersAsync(int userId)
        {
            // Get all unique users who have exchanged messages with this user
            var userIds = await _dbSet
                .Where(pm => pm.SenderID == userId || pm.ReceiverID == userId)
                .Select(pm => pm.SenderID == userId ? pm.ReceiverID : pm.SenderID)
                .Distinct()
                .ToListAsync();

            var _UserDbSet= _context.Set<User>();
            var users = await _UserDbSet
                .Where(u => userIds.Contains(u.UserID))
                .ToListAsync();
            var conversations = new List<ConversationDto>();

            foreach (var otherUser in users)
            {
                // Get the most recent message between these users
                var lastMessage = await _dbSet
                    .Where(pm =>
                        (pm.SenderID == userId && pm.ReceiverID == otherUser.UserID) ||
                        (pm.SenderID == otherUser.UserID && pm.ReceiverID == userId))
                    .OrderByDescending(pm => pm.CreatedAt)
                    .FirstOrDefaultAsync();

                // Count unread messages from the other user
                var unreadCount = await GetUnreadMessageCountFromSenderAsync(otherUser.UserID, userId);

                if (lastMessage != null)
                {
                    conversations.Add(new ConversationDto
                    {
                        UserId = otherUser.UserID,
                        UserName = otherUser.UserName,
                        FullName = otherUser.FullName,
                        Avatar = otherUser.Avatar,
                        LastMessage = lastMessage.Content,
                        LastMessageTime = lastMessage.CreatedAt,
                        UnreadCount = unreadCount,
                        IsOnline = false // This would be set by the online user tracking system
                    });
                }
            }
            return conversations;
        }
    }
}
