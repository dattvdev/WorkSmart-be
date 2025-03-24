using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;
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
                .Where(a => a.JobID == jobId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Application>> GetApplicationsByUserIdAsync(int userId)
        {
            return await _context.Applications
                .Include(a => a.Job)
                .Where(a => a.UserID == userId)  // Lọc theo UserID
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
            application.UpdatedAt = DateTime.Now;

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
                    .ThenInclude(cv => cv.Educations)
                .Include(a => a.CV)
                    .ThenInclude(cv => cv.Experiences)
                .Include(a => a.CV)
                    .ThenInclude(cv => cv.Skills)
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
    }
}
