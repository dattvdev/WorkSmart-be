using Microsoft.EntityFrameworkCore;
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
    }
}
