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

        public async Task<List<JobAlert>> GetJobAlertsByJobId(int jobId)
        {
            var job = await _context.Set<Job>().FirstOrDefaultAsync(j => j.JobID == jobId);
            if (job == null)
            {
                return new List<JobAlert>();
            }

            var alerts = await _dbSet
                .Include(a => a.User)
                .Where(alert =>
                    (string.IsNullOrEmpty(alert.Experience) || alert.Experience == job.Exp.ToString()) &&
                    (string.IsNullOrEmpty(alert.JobType) || alert.JobType == job.WorkType) &&
                    (string.IsNullOrEmpty(alert.SalaryRange) || alert.SalaryRange == job.Salary.ToString()) &&
                    (string.IsNullOrEmpty(alert.Keyword) || job.Title.ToLower().Contains(alert.Keyword.ToLower()))
                )
                .ToListAsync();

            var matchingAlerts = alerts
                .Where(alert =>
                    string.IsNullOrEmpty(alert.Province) ||
                    job.Location
                        .Split(',',StringSplitOptions.RemoveEmptyEntries)
                        .Any(loc => loc.Trim().Equals(alert.Province.Trim(),StringComparison.OrdinalIgnoreCase))
                )
                .ToList();


            return matchingAlerts;
        }

    }
}
