using Microsoft.EntityFrameworkCore;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class JobRepository : IJobRepository
    {
        private readonly WorksmartDBContext _context;

        public JobRepository(WorksmartDBContext context)
        {
            _context = context;
        }

        public async Task<Job> CreateJobAsync(Job job)
        {
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<Job> UpdateJobAsync(Job job)
        {
            _context.Jobs.Update(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<bool> DeleteJobAsync(int jobId)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null) return false;

            _context.Jobs.Remove(job);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Job> GetJobByIdAsync(int jobId)
        {
            return await _context.Jobs
                .Include(j => j.User)
                .Include(j => j.JobTag)
                .FirstOrDefaultAsync(j => j.JobID == jobId);
        }

        public async Task<List<Job>> GetAllJobsAsync()
        {
            return await _context.Jobs
                .Include(j => j.User)
                .Include(j => j.JobTag)
                .ToListAsync();
        }

        public async Task<bool> ApproveJobAsync(int jobId)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null) return false;

            job.Status = JobStatus.Approved;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HideJobAsync(int jobId)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null) return false;

            job.Status = JobStatus.Hidden;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}