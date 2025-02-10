using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class JobService
    {
        private readonly IJobRepository _jobRepository;

        public JobService(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<Job> CreateJobAsync(Job job) => await _jobRepository.CreateJobAsync(job);

        public async Task<Job> UpdateJobAsync(Job job) => await _jobRepository.UpdateJobAsync(job);

        public async Task<bool> DeleteJobAsync(int jobId) => await _jobRepository.DeleteJobAsync(jobId);

        public async Task<Job> GetJobByIdAsync(int jobId) => await _jobRepository.GetJobByIdAsync(jobId);

        public async Task<List<Job>> GetAllJobsAsync() => await _jobRepository.GetAllJobsAsync();

        public async Task<bool> ApproveJobAsync(int jobId) => await _jobRepository.ApproveJobAsync(jobId);

        public async Task<bool> HideJobAsync(int jobId) => await _jobRepository.HideJobAsync(jobId);
    }
}
