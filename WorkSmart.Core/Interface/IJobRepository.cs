using System.Collections.Generic;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;

namespace WorkSmart.Core.Interface
{
    public interface IJobRepository : IBaseRepository<Job>
    {
        Task<Job> GetJobDetail(int id);
        Task<IEnumerable<Job>> GetJobsByEmployerId(int employerId);
        Task<IEnumerable<Job>> GetJobsByStatus(JobStatus status);
        Task<bool> UpdateJobStatus(int jobId, JobStatus newStatus);
        Task<(IEnumerable<Job> Jobs, int Total)> GetListSearch(JobSearchRequestDto request);
        Task<(IEnumerable<Job> Jobs, int Total)> GetJobsForManagement(JobSearchRequestDto request);
        Task<bool> HideJobAsync(int jobId);
        Task<bool> UnhideJobAsync(int jobId);
        Task<List<Job>> GetExpiredJobsAsync();
        Task<List<Job>> HideExpiredJobsAsync();
        Task<List<Job>> GetSimilarJob(int jobId);
        Task<Job> GetByJobId(int jobId);
        Task<bool> ApproveJobAsync(int jobId);
        Task<bool> RejectJobAsync(int jobId, string reason);
        Task<bool> CheckLimitCreateJob(int userID, int? maxJobsPerDayFromClient);
        Task<JobCreationLimitDto> GetRemainingJobCreationLimit(int userID);
        Task<bool> CheckLimitCreateFeaturedJob(int userID);
        Task<JobPriorityLimitDto> GetRemainingJobPriorityLimit(int userID);
        Task<bool> ToggleJobPriorityAsync(int jobId);
        Task<bool> UnPriorityAsync(int jobId);
        Task<IEnumerable<object>> JobCategoryDashboard();
        Task<IEnumerable<object>> JobStatusDashboard();
        Task<IEnumerable<object>> JobLocationDashboard();
        Task<List<Job>> GetExpiringJobsAsync();
        Task<List<Job>> GetExpiredJobs();
        Task<IEnumerable<int>> GetJobIdsByUserIdAsync(int userId);
        Task<IEnumerable<Job>> GetJobsActive();
        Task<List<Job>> GetAllJobActive();
        Task<bool> IsDuplicateJobTitle(int userID, string normalizedTitle);
        Task<bool> IsDuplicateJobTitleForUpdate(int userID, int jobID, string normalizedTitle);
        Task<object> TopCategoryJob();
    }
}
