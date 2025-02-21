using System.Collections.Generic;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;

namespace WorkSmart.Core.Interface
{
    public interface IJobRepository : IBaseRepository<Job>
    {
        Task<IEnumerable<Job>> GetJobsByEmployerId(int employerId);
        Task<IEnumerable<Job>> GetJobsByStatus(JobStatus status);
        Task<bool> UpdateJobStatus(int jobId, JobStatus newStatus);
    }
}
