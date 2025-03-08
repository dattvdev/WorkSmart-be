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
    public class SubscriptionRepository : BaseRepository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(WorksmartDBContext context) : base(context)
        {
        }

        public async Task<Subscription> GetByUserId(int id)
        {
           return await _dbSet.Where(x => x.UserID == id).FirstOrDefaultAsync();
        }
    }
}
