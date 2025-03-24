using Microsoft.EntityFrameworkCore;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class NotificationJobTagRepository : BaseRepository<NotificationJobTag>, INotificationJobTagRepository
    {
        public NotificationJobTagRepository(WorksmartDBContext context) : base(context)
        {
            
        }
        public async Task<List<string> > GetUserIDByTagID(List<int> tagIDs)
        {
            return await _dbSet.Where(x => tagIDs.Contains(x.TagID))
                .Select(x => x.Email)
                .Distinct()
                .ToListAsync();
        }
    }
}
