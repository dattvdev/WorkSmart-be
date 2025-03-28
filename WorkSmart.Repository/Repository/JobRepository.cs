using Microsoft.EntityFrameworkCore;
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

        public async Task<bool> ApproveJobAsync(int jobId)
        {
            var job = await _dbSet.FindAsync(jobId);
            if (job == null) return false;

            job.Status = JobStatus.Active;
            //job.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RejectJobAsync(int jobId, string reason)
        {
            var job = await _dbSet.FindAsync(jobId);
            if (job == null) return false;

            job.Status = JobStatus.Rejected;
            job.ReasonRejectedJob = reason; // Store the rejection reason
            job.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
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
            return await _dbSet.Include(j => j.User).Include(t => t.Tags).FirstOrDefaultAsync(c => c.JobID == id);
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
            query = query.Where(c => c.Status == JobStatus.Active);
            if (!string.IsNullOrWhiteSpace(request.Category) && !request.Category.Equals("All Categories"))
            {
                query = query.Where(c => c.CategoryID.Contains(request.Category)); 
            }

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                query = query.Where(c => c.Title.ToLower().Contains(request.Title.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(request.JobPosition))
            {
                query = query.Where(c => c.JobPosition.ToLower().Contains(request.JobPosition.ToLower()));
            }

            if (request.WorkTypes != null && request.WorkTypes.Any())
            {
                query = query.Where(c => request.WorkTypes.Select(wt => wt.ToLower()).Contains(c.WorkType.ToLower()));

            }

            if (!string.IsNullOrWhiteSpace(request.Location))
            {
                query = query.Where(c => c.Location.ToLower().Contains(request.Location.ToLower()));
            }

            if (request.Tags != null && request.Tags.Any())
            {
                query = query.Where(c => c.Tags.Any(t => request.Tags.Contains(t.TagID)));
            }

            if (request.MostRecent)
            {
                query = query.OrderByDescending(c => c.UpdatedAt);
            }

            // Tải dữ liệu về bộ nhớ trước khi xử lý Salary
            var jobList = await query.ToListAsync();

            if (request.MinSalary.HasValue)
            {
                double minSalary = request.MinSalary.Value;
                jobList = jobList.Where(c => c.Salary != null
                    && c.Salary.Contains("-")
                    && c.Salary.Split('-').Length == 2
                    && double.TryParse(c.Salary.Split('-')[0], out double min)
                    && min >= minSalary).ToList();
            }

            /*if (request.MaxSalary.HasValue)
            {
                double maxSalary = request.MaxSalary.Value;
                jobList = jobList.Where(c => c.Salary != null
                    && c.Salary.Contains("-")
                    && c.Salary.Split('-').Length == 2
                    && double.TryParse(c.Salary.Split('-')[1], out double max)
                    && max <= maxSalary).ToList();
            }*/
            // Lấy tổng số bản ghi trước khi phân trang
            int total = jobList.Count();

            var Jobs = jobList
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

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

            if (request.Tags != null && request.Tags.Any())
            {
                query = query.Where(c => c.Tags.Any(t => request.Tags.Contains(t.TagID)));
            }

            if (!request.MostRecent)
            {
                query = query.OrderByDescending(c => c.UpdatedAt);
            }
           
            // Tải dữ liệu về bộ nhớ trước khi xử lý Salary
            var jobList = await query.ToListAsync();

            if (request.MinSalary.HasValue)
            {
                double minSalary = request.MinSalary.Value;
                jobList = jobList.Where(c => c.Salary != null
                    && c.Salary.Contains("-")
                    && c.Salary.Split('-').Length == 2
                    && double.TryParse(c.Salary.Split('-')[0], out double min)
                    && min >= minSalary).ToList();
            }

            /*if (request.MaxSalary.HasValue)
            {
                double maxSalary = request.MaxSalary.Value;
                jobList = jobList.Where(c => c.Salary != null
                    && c.Salary.Contains("-")
                    && c.Salary.Split('-').Length == 2
                    && double.TryParse(c.Salary.Split('-')[1], out double max)
                    && max <= maxSalary).ToList();
            }*/
            // Lấy tổng số bản ghi trước khi phân trang
            int total = jobList.Count();

            var Jobs = jobList
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

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
                .Where(j => j.Deadline < currentDate)
                .ToListAsync();
        }

        public async Task<List<Job>> HideExpiredJobsAsync()
        {
            var expiredJobs = await GetExpiredJobsAsync();

            foreach (var job in expiredJobs)
            {
                job.Status = JobStatus.Hidden;
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

        public async Task<List<Job>> GetSimilarJob(int jobId)
        {
            var job = await _dbSet.Include(j => j.Tags).Include(u => u.User)
                      .FirstOrDefaultAsync(j => j.JobID == jobId);

            if (job == null || job.Tags == null || !job.Tags.Any())
                return new List<Job>(); // Trả về danh sách rỗng nếu không tìm thấy job hoặc không có Tags

            var jobTagIds = job.Tags.Select(t => t.TagID).ToList(); // Lấy danh sách TagID của job

            var similarJobs = _dbSet.Include(j => j.Tags)
                .Where(j => j.JobID != jobId)
                .Where(j => j.Status == JobStatus.Active)
                .AsEnumerable() // Chuyển truy vấn về bộ nhớ
                .Where(j => j.Tags.Any(t => jobTagIds.Contains(t.TagID))) // Lọc các job có Tag trùng
                .OrderByDescending(j => j.Tags.Count)
                .Take(3)
                .ToList(); // Đổi ToListAsync() thành ToList()

            return similarJobs;
        }
        public async Task<Job> GetByJobId(int id)
        {
            return await _dbSet.Include(t => t.Tags).FirstOrDefaultAsync(j => j.JobID == id);
        }
    }
}
