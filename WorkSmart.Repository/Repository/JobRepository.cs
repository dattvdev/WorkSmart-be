using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class JobRepository : BaseRepository<Job>, IJobRepository
    {
        public JobRepository(WorksmartDBContext context) : base(context)
        {
        }

        public Task<bool> ApproveJobAsync(int jobId)
        {
            throw new NotImplementedException();
        }

        public Task<Job> CreateJobAsync(Job job)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteJobAsync(int jobId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Job>> GetAllJobsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Job> GetJobByIdAsync(int jobId)
        {
            throw new NotImplementedException();
        }


        public async Task<IEnumerable<Job>> GetJobsByEmployerId(int employerId)
        {
            return await _dbSet.Where(j => j.UserID == employerId).ToListAsync();
        }


        public async Task<IEnumerable<Job>> GetJobsByStatus(JobStatus status)
        {
            return await _dbSet.Where(j => j.Status == status).ToListAsync();
        }

        public Task<bool> HideJobAsync(int jobId)
        {
            throw new NotImplementedException();
        }

        public Task<Job> UpdateJobAsync(Job job)
        {
            throw new NotImplementedException();
        }

        
        public async Task<bool> UpdateJobStatus(int jobId, JobStatus newStatus)
        {
            var job = await _dbSet.FindAsync(jobId);
            if (job == null) return false;

            job.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
