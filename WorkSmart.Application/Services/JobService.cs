using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;
using WorkSmart.Core.Interface;
using WorkSmart.Repository.Repository;

namespace WorkSmart.Application.Services
{
    public class JobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly IMapper _mapper;
        private readonly ITagRepository _tagRepository;

        public JobService(IJobRepository jobRepository, IMapper mapper, ITagRepository tagRepository)
        {
            _jobRepository = jobRepository;
            _mapper = mapper;
            _tagRepository = tagRepository;
        }

        public async Task<JobDto> GetJobById(int jobId)
        {
            var job = await _jobRepository.GetById(jobId);
            return job == null ? null : _mapper.Map<JobDto>(job);
        }
        public async Task<IEnumerable<JobDto>> GetAllJobsAsync()
        {
            var jobs = await _jobRepository.GetAll();
            var jobOrderBy = jobs.OrderByDescending(j => j.CreatedAt).ToList();
            return _mapper.Map<IEnumerable<JobDto>>(jobOrderBy);
        }
        public async Task CreateJobAsync(CreateJobDto jobDto)
        {
            var job = _mapper.Map<Job>(jobDto);
            var allTags = await _tagRepository.GetAll();
            if (jobDto.JobTagID != null && jobDto.JobTagID.Any())
            {
                job.Tags = allTags.Where(t => jobDto.JobTagID.Contains(t.TagID)).ToList();
            }
            await _jobRepository.Add(job);
        }

        public async Task<JobDto> UpdateJobAsync(int jobId, UpdateJobDto jobDto)
        {
            var job = await _jobRepository.GetById(jobId);
            if (job == null) return null;

            _mapper.Map(jobDto, job);
            await _jobRepository.Save();
            return _mapper.Map<JobDto>(job);
        }
        public void DeleteJob(int jobId)
        {
            _jobRepository.Delete(jobId);
        }

        public async Task<bool> HideJob(int jobId) => await _jobRepository.UpdateJobStatus(jobId, JobStatus.Hidden);
        public async Task<bool> UnhideJob(int jobId) => await _jobRepository.UpdateJobStatus(jobId, JobStatus.Active);
        public async Task<(IEnumerable<GetListSearchJobDto> Jobs, int Total)> GetListSearch(JobSearchRequestDto request)
        {
            var (jobs, total) = await _jobRepository.GetListSearch(request);

            var mappedJobs = _mapper.Map<IEnumerable<GetListSearchJobDto>>(jobs);

            return (mappedJobs, total);
        }
    }
}
