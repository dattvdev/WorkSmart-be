using System.Collections.Generic;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;
namespace WorkSmart.Core.Interface
{
    public interface IApplicationRepository : IBaseRepository<Application>
    {
        Task<IEnumerable<Application>> GetApplicationsByJobIdAsync(int jobId);
        Task<bool> UpdateApplicationStatusAsync(int applicationId, string status);
        Task<IEnumerable<Application>> GetApplicationsByUserIdAsync(int userId);
        Task<Application> GetCandidateByIdAsync(int candidateId);
        Task ApplyToJob(int userId, int jobId);
        Task<bool> UpdateRejectionReasonAsync(int applicationId, string rejectionReason);
        Task<Job> GetJobDetailForApplicationAsync(int applicationId);
        Task<Application> GetApplicationDetailAsync(int applicationId, int jobId);
        string CheckApplyJob(int UserId, int JobId);
        void ChangeCV(int applicationId, int cvId);
        Task<IEnumerable<object>> ApplicationCountDashboard();
        Task<IEnumerable<Application>> GetApplicationsByJobIdsAsync(IEnumerable<int> jobIds);
        Task<bool> UpdateInterviewDetailsAsync(int applicationId, InterviewInvitationRequestDto request);
        Task<bool> WithdrawApplicationAsync(int userId, int jobId);
        Task<Application> GetApplicationDetails(int userId, int jobId);
        Task<bool> CheckReApplyJob(int userId, int jobId);
    }
}