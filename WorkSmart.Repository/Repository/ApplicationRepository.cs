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

        public async Task ApplyToJob(int userId, int jobId, int cvId)
        {
            Application application = new Application
            {
                UserID = userId,
                JobID = jobId,
                CVID = cvId,
                Status = "Pending",
            };
            await Add(application);
        }

        // Lấy danh sách ứng viên đã ứng tuyển cho công việc với jobId
        public async Task<IEnumerable<Application>> GetApplicationsByJobIdAsync(int jobId)
        {
            return await _context.Applications
                .Where(a => a.JobID == jobId) 
                .Include(a => a.User)  
                .Include(a => a.CV)  
                .ToListAsync();
        }

        public async Task<IEnumerable<Application>> GetApplicationsByUserIdAsync(int userId)
        {
            return await _context.Applications
                .Where(a => a.UserID == userId)  // Lọc theo UserID
                .Include(a => a.Job) 
                .ToListAsync();
        }
        public async Task<Application> GetCandidateByIdAsync(int candidateId)
        {
            return await _context.Applications
                .Where(a => a.ApplicationID == candidateId)
                .Include(a => a.User) 
                .FirstOrDefaultAsync(); 
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

        Task<IEnumerable<Application>> IApplicationRepository.GetApplicationsByUserIdAsync(int userId)
        {
            throw new NotImplementedException();
        }


    }
}
