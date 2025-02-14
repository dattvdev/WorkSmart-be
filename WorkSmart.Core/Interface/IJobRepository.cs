using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface IJobRepository
    {
        Task<Job> CreateJobAsync(Job job);
        Task<Job> UpdateJobAsync(Job job);
        Task<bool> DeleteJobAsync(int jobId);
        Task<Job> GetJobByIdAsync(int jobId);
        Task<List<Job>> GetAllJobsAsync();
        Task<bool> ApproveJobAsync(int jobId);
        Task<bool> HideJobAsync(int jobId);
    }
}
