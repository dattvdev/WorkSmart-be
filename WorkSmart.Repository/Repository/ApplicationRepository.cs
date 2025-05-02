using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Helpers;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class ApplicationRepository : BaseRepository<Application>, IApplicationRepository
    {
        public ApplicationRepository(WorksmartDBContext context) : base(context)
        {
        }

        public async Task ApplyToJob(int userId, int jobId)
        {
            var cv = _context.CVs.FirstOrDefault(cv => cv.UserID == userId && cv.IsFeatured==true);
            Application application = new Application
            {
                UserID = userId,
                JobID = jobId,
                CVID = cv.CVID,
                Status = "Pending",
            };
            await Add(application);
        }

        // Lấy danh sách ứng viên đã ứng tuyển cho công việc với jobId
        public async Task<IEnumerable<Application>> GetApplicationsByJobIdAsync(int jobId)
        {
            return await _context.Applications
                .Include(a => a.User)  
                .Include(a => a.CV)
                .Include(a => a.Job)
                .Include(a => a.Job.User)
                .Include(a => a.CV.User)
                .Where(a => a.JobID == jobId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Application>> GetApplicationsByUserIdAsync(int userId)
        {
            return await _context.Applications
                .Include(a => a.Job)
                    .ThenInclude(b => b.User)
                .Include(c => c.User)
                .Where(a => a.UserID == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Application> GetCandidateByIdAsync(int candidateId)
        {
            return await _context.Applications
                .Include(a => a.User)
                .Where(a => a.ApplicationID == candidateId)
                .FirstOrDefaultAsync(); 
        }

       
        public async Task<Job> GetJobDetailForApplicationAsync(int applicationId)
        {
            var application = await _context.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);

            return application?.Job;
        }

        public async Task<bool> UpdateApplicationStatusAsync(int applicationId, string status)
        {
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);

            if (application == null)
            {
                return false; 
            }

            application.Status = status;  
            await _context.SaveChangesAsync(); 
            return true;
        }

        public async Task<bool> UpdateRejectionReasonAsync(int applicationId, string rejectionReason)
        {
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);

            if (application == null)
            {
                return false;
            }

            application.RejectionReason = rejectionReason?.Trim();
            application.UpdatedAt = TimeHelper.GetVietnamTime();

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Application> GetApplicationDetailAsync(int applicationId, int jobId)
        {
            return await _context.Applications
                 .Include(a => a.User)
                 .Include(a => a.CV)
                 .Include(a => a.Job)
                 .Include(a => a.Job.User)
                 .Include(a => a.CV.User)
                 .Where(a => a.JobID == jobId)
                 .FirstOrDefaultAsync(a => a.ApplicationID == applicationId && a.JobID == jobId);
        }

        public string CheckApplyJob(int UserId, int JobId)
        {
            var apply =_dbSet.Where(a => a.UserID == UserId && a.JobID == JobId).FirstOrDefault();
            if (apply != null)
            {
                return apply.Status;
            }
            return "None";
        }

        public void ChangeCV(int applicationId, int cvId)
        {
            var application = _dbSet.FirstOrDefault(a => a.ApplicationID == applicationId);
            if (application != null)
            {
                application.CVID = cvId;
                _dbSet.Update(application);
                _context.SaveChanges();
            }
        }
        public async Task<IEnumerable<object>> ApplicationCountDashboard()
        {
            var currentWeekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek + 1);
            var currentWeekEnd = currentWeekStart.AddDays(6);

            var applications =  _context.Applications
                .Where(a => a.CreatedAt >= currentWeekStart && a.CreatedAt <= currentWeekEnd)
                .AsEnumerable() // Chuyển sang xử lý trên bộ nhớ
                .GroupBy(a => a.CreatedAt.DayOfWeek)
                .Select(g => new
                {
                    Day = g.Key.ToString(), // Chuyển thành tên ngày (Monday, Tuesday,...)
                    Applications = g.Count()
                })
                .ToList();

            var orderedData = new[]
            {
                "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
            }.Select(day => new
            {
                Day = day,
                Applications = applications.FirstOrDefault(a => a.Day == day)?.Applications ?? 0
            });

            return orderedData;
        }

        public async Task<IEnumerable<Application>> GetApplicationsByJobIdsAsync(IEnumerable<int> jobIds)
        {
            return await _context.Applications
                .Include(a => a.User)
                .Include(a => a.CV)
                .Where(a => jobIds.Contains(a.JobID))
                .ToListAsync();
        }
        public async Task<bool> UpdateInterviewDetailsAsync(int applicationId, InterviewInvitationRequestDto request)
        {
            var application = await _context.Applications
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);

            if (application == null)
            {
                return false;
            }

            application.UpdatedAt = TimeHelper.GetVietnamTime();

            // Lưu thông tin phỏng vấn
            // Nếu có bảng Interview riêng, bạn có thể tạo và lưu thông tin ở đây
            // Hoặc có thể lưu metadata dưới dạng JSON thông qua một trường mở rộng

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> WithdrawApplicationAsync(int userId, int jobId)
        {
            var status = await GetApplicationDetails(userId, jobId);
            if (status.Status == "Pending")
            {
                if (status == null)
                    return false;
                _dbSet.Remove(status);
                await _context.SaveChangesAsync();
            }
            else
            {
                return false; // Không thể rút đơn nếu không phải trạng thái "Pending"
            }
            return true;
        }
        public async Task<Application> GetApplicationDetails(int userId, int jobId)
        {
            return await _context.Applications
                .Where(a => a.UserID == userId && a.JobID == jobId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CheckReApplyJob(int userId, int jobId)
        {
            var application = await GetApplicationDetails(userId, jobId);
            if (application == null) return true; // Chưa ứng tuyển
            if (application.Status == "Approved") return false;
            var applications = await _context.Applications.Where(Task => Task.UserID == userId && Task.JobID == jobId).ToListAsync();
            return applications.Count() < 3;
        }

    }
}
