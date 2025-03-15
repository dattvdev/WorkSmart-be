﻿using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.JobDtos;
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

        public async Task<Job> GetJobDetail(int id)
        {
            return await _dbSet.Include(j => j.User).FirstOrDefaultAsync(c => c.JobID == id);
        }

        public async Task<IEnumerable<Job>> GetJobsByEmployerId(int employerId)
        {
            return await _dbSet.Where(j => j.UserID == employerId).ToListAsync();
        }

        public async Task<bool> HideJobAsync(int jobId)
        {
            var job = await _dbSet.FindAsync(jobId);
            if (job == null) return false;

            job.Status = JobStatus.Hidden;
            job.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Job>> GetJobsByStatus(JobStatus status)
        {
            return await _dbSet.Where(j => j.Status == status).ToListAsync();
        }

        public async Task<(IEnumerable<Job> Jobs, int Total)> GetListSearch(JobSearchRequestDto request)
        {
            DbSet<Job> _JobdbSet = _context.Set<Job>();
            var query = _JobdbSet.Include(c => c.User).AsQueryable();

            // Filter by title
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                query = query.Where(c => c.Title.ToLower().Contains(request.Title.ToLower()));
            }

            // Filter by job position
            if (!string.IsNullOrWhiteSpace(request.JobPosition))
            {
                query = query.Where(c => c.JobPosition.ToLower().Contains(request.JobPosition.ToLower()));
            }

            // Filter by work types
            if (request.WorkTypes != null && request.WorkTypes.Any())
            {
                query = query.Where(c => request.WorkTypes.Select(wt => wt.ToLower()).Contains(c.WorkType.ToLower()));
            }

            // Filter by location
            if (!string.IsNullOrWhiteSpace(request.Location))
            {
                query = query.Where(c => c.Location.ToLower().Contains(request.Location.ToLower()));
            }

            // Filter by salary range
            if (request.MinSalary.HasValue)
            {
                query = query.Where(c => c.Salary >= request.MinSalary);
            }
            if (request.MaxSalary.HasValue)
            {
                query = query.Where(c => c.Salary <= request.MaxSalary);
            }

            // Filter by tags
            if (request.Tags != null && request.Tags.Any())
            {
                query = query.Where(c => c.Tags.Any(t => request.Tags.Contains(t.TagID)));
            }

            //// Filter out hidden jobs
            query = query.Where(c => c.Status != JobStatus.Hidden);

            // Apply sorting
            if (!request.MostRecent)
            {
                query = query.OrderByDescending(c => c.UpdatedAt);
            }
            else
            {
                query = query.OrderByDescending(c => c.CreatedAt);
            }

            // Get total count before pagination
            int total = await query.CountAsync();

            // Apply pagination
            var Jobs = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            if (!Jobs.Any())
            {
                return (new List<Job>(), total);
            }

            return (Jobs, total);
        }
        public async Task<(IEnumerable<Job> Jobs, int Total)> GetJobsForManagement(JobSearchRequestDto request)
        {
            DbSet<Job> _JobdbSet = _context.Set<Job>();
            var query = _JobdbSet.Include(c => c.User).AsQueryable();

            // Filter by title
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                query = query.Where(c => c.Title.ToLower().Contains(request.Title.ToLower()));
            }

            // Filter by job position
            if (!string.IsNullOrWhiteSpace(request.JobPosition))
            {
                query = query.Where(c => c.JobPosition.ToLower().Contains(request.JobPosition.ToLower()));
            }

            // Filter by work types
            if (request.WorkTypes != null && request.WorkTypes.Any())
            {
                query = query.Where(c => request.WorkTypes.Select(wt => wt.ToLower()).Contains(c.WorkType.ToLower()));
            }

            // Filter by location
            if (!string.IsNullOrWhiteSpace(request.Location))
            {
                query = query.Where(c => c.Location.ToLower().Contains(request.Location.ToLower()));
            }

            // Filter by salary range
            if (request.MinSalary.HasValue)
            {
                query = query.Where(c => c.Salary >= request.MinSalary);
            }
            if (request.MaxSalary.HasValue)
            {
                query = query.Where(c => c.Salary <= request.MaxSalary);
            }

            // Filter by tags
            if (request.Tags != null && request.Tags.Any())
            {
                query = query.Where(c => c.Tags.Any(t => request.Tags.Contains(t.TagID)));
            }
            // Apply sorting
            if (!request.MostRecent)
            {
                query = query.OrderByDescending(c => c.UpdatedAt);
            }
            else
            {
                query = query.OrderByDescending(c => c.CreatedAt);
            }

            // Get total count before pagination
            int total = await query.CountAsync();

            // Apply pagination
            var Jobs = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            if (!Jobs.Any())
            {
                return (new List<Job>(), total);
            }

            return (Jobs, total);
        }
        public async Task<bool> UnhideJobAsync(int jobId)
        {
            var job = await _dbSet.FindAsync(jobId);
            if (job == null) return false;

            // Revert to previous status or set to Active
            job.Status = JobStatus.Active;
            job.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
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
        public async Task<List<Job>> GetExpiredJobsAsync()
        {
            var currentDate = DateTime.Now;
            return await _dbSet
                .Where(j => j.Deadline < currentDate && j.Status == JobStatus.Active)
                .ToListAsync();
        }

        public async Task<List<Job>> HideExpiredJobsAsync()
        {
            var expiredJobs = await GetExpiredJobsAsync();

            foreach (var job in expiredJobs)
            {
                job.Status = JobStatus.Hidden;
                job.UpdatedAt = DateTime.Now;
            }

            if (expiredJobs.Any())
            {
                await _context.SaveChangesAsync();
            }

            return expiredJobs;
        }


        public async Task<Job> GetJobDetailForApplicationAsync(int applicationId)
        {
            var application = await _context.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);

            return application?.Job;
        }
    }
}
