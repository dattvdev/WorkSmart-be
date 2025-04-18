using Microsoft.EntityFrameworkCore;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class JobAlertRepository : BaseRepository<JobAlert>, IJobAlertRepository
    {
        public JobAlertRepository(WorksmartDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<JobAlert>> GetJobAlertsByUserId(int userId)
        {
            return await _dbSet.Where(x => x.UserId == userId).ToListAsync();
        }
    }
}
