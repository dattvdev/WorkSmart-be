using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(WorksmartDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetByUserId(int userId)
        {
            return await _dbSet.Where(x => x.UserID == userId).ToListAsync();
        }
    }
   
}
