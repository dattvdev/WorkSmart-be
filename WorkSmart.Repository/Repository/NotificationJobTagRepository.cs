using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.UserDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class NotificationJobTagRepository : BaseRepository<NotificationJobTag>, INotificationJobTagRepository
    {
        public NotificationJobTagRepository(WorksmartDBContext context) : base(context)
        {
            
        }
        public async Task<List<UserNotificationTagDto>> GetNotiUserByListTagID(List<int> tagIDs)
        {
            if (tagIDs == null || !tagIDs.Any())
            {
                return new List<UserNotificationTagDto>();
            }

            var result = await _dbSet
                .Where(njt => tagIDs.Contains(njt.TagID))
                .GroupBy(njt => new
                {
                    njt.UserID,
                    njt.Email,
                    njt.User.UserName,
                    njt.User.FullName
                })
                .Select(g => new UserNotificationTagDto
                {
                    UserID = g.Key.UserID,
                    UserName = g.Key.UserName,  // Lấy trực tiếp từ GroupBy
                    FullName = g.Key.FullName,
                    Email = g.Key.Email,
                    TagNames = g.Select(x => x.Tag.TagName).ToList()
                })
                .ToListAsync();

            return result;
        }
        
        public async Task<List<UserNotificationTagManageDto>> GetListRegisterTag(int UserID)
        {
            var result = await _dbSet.Include(njt => njt.Tag)
                .Where(njt => njt.UserID == UserID)
                .GroupBy(njt => njt.Tag.CategoryID)
                .Select(g => new UserNotificationTagManageDto
                {
                    Category = g.Key, // Nếu CategoryID là int
                    EmailTags = g.GroupBy(x => x.Email) // Nhóm tiếp theo Email
                        .Select(emailGroup => new EmailTag
                        {
                            Email = emailGroup.Key,
                            Tags = emailGroup.Select(x => x.Tag).ToList()
                        })
                        .ToList()
                })  
                .ToListAsync();

            return result;
        }   

        public async Task AddNotificationTag(int userId, List<int> tagId, string email)
        {
            foreach (var tag in tagId)
            {
                var notificationJobTag = new NotificationJobTag
                {
                    UserID = userId,
                    TagID = tag,
                    Email = email
                };
                if (!_dbSet.Any(njt => njt.UserID == userId && njt.TagID == tag && njt.Email == email))
                await _dbSet.AddAsync(notificationJobTag);
            }
            await _context.SaveChangesAsync();
        }

        public void DeleteByCategory(int userId, string categoryID)
        {
            var notificationJobTags = _dbSet.Include(c => c.Tag)
                                            .Where(njt => njt.UserID == userId && njt.Tag.CategoryID.Equals(categoryID))
                                            .ToList();
            _dbSet.RemoveRange(notificationJobTags);
            _context.SaveChanges();
        }

        public void DeleteByCategoryEmail(int userId, string categoryID, string email)
        {
            var notificationJobTags = _dbSet.Include(c => c.Tag)
                                             .Where(njt => njt.UserID == userId
                                                        && njt.Tag.CategoryID == categoryID
                                                        && njt.Email == email)
                                             .ToList();

            if (!notificationJobTags.Any())  // Kiểm tra danh sách có rỗng không
            {
                return;
            }

            _dbSet.RemoveRange(notificationJobTags);
            _context.SaveChanges();
        }

        public void DeleteByCategoryEmailTag(int userId, string categoryID, string email, int tagIds)
        {
            var notificationJobTags = _dbSet.Include(c => c.Tag)
                                             .FirstOrDefault(njt => njt.UserID == userId
                                                                 && njt.Tag.CategoryID == categoryID
                                                                 && njt.Email == email
                                                                 && njt.TagID == tagIds);

            if (notificationJobTags == null)
            {
                return;
            }

            _dbSet.Remove(notificationJobTags);
            _context.SaveChanges();
        }

        public async Task<List<int>> GetListTagIdByEmail(int userId, string email)
        {
            return await _dbSet.Where(njt => njt.UserID == userId && njt.Email == email)
                               .Select(njt => njt.TagID)
                               .ToListAsync();
        }

    }
}
