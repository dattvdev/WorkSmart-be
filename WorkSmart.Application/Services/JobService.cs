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
        private readonly JobRecommendationService _recommendationService;
        public JobService(IJobRepository jobRepository
            , IMapper mapper
            , ITagRepository tagRepository
            , INotificationJobTagRepository notificationJobTagRepository
            , JobRecommendationService recommendationService)
        {
            _jobRepository = jobRepository;
            _mapper = mapper;
            _tagRepository = tagRepository;
            _recommendationService = recommendationService;
        }

        public async Task<(JobDetailDto, IEnumerable<JobDetailDto> similarJobs)> GetJobById(int jobId)
        {
            var job = await _jobRepository.GetJobDetail(jobId);
            var similarJobs = await _jobRepository.GetSimilarJob(jobId);
            return (_mapper.Map<JobDetailDto>(job), _mapper.Map<IEnumerable<JobDetailDto>>(similarJobs));
        }

        public async Task<(IEnumerable<GetListSearchJobDto> Jobs, int Total)> GetJobsForManagement(JobSearchRequestDto request)
        {
            var (jobs, total) = await _jobRepository.GetJobsForManagement(request);
            var mappedJobs = _mapper.Map<IEnumerable<GetListSearchJobDto>>(jobs);
            return (mappedJobs, total);
        }

        public async Task<IEnumerable<JobDto>> GetAllJobsAsync()
        {
            var jobs = await _jobRepository.GetAll();
            var jobOrderBy = jobs.OrderByDescending(j => j.CreatedAt).ToList();
            return _mapper.Map<IEnumerable<JobDto>>(jobOrderBy);
        }

        public async Task<IEnumerable<JobActiveDto>> GetJobsActive()
        {
            var jobs = await _jobRepository.GetJobsActive();
            var jobOrderBy = jobs.OrderByDescending(j => j.CreatedAt).ToList();
            return _mapper.Map<IEnumerable<JobActiveDto>>(jobOrderBy);
        }

        //public async Task CreateJobAsync(CreateJobDto jobDto)
        //{
        //    var job = _mapper.Map<Job>(jobDto);
        //    var allTags = await _tagRepository.GetAll();
        //    if (jobDto.JobTagID != null && jobDto.JobTagID.Any())
        //    {
        //        job.Tags = allTags.Where(t => jobDto.JobTagID.Contains(t.TagID)).ToList();
        //    }
        //    await _jobRepository.Add(job);
        //}
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
            var job = await _jobRepository.GetByJobId(jobId);
            if (job == null) return null;
            var tags = await _tagRepository.GetTagByListID(jobDto.Tags);
            if (job.Tags != null)
            {
                job.Tags.Clear();
            }
            _mapper.Map(jobDto, job, opts => opts.Items["Tags"] = tags);

            job.Status = JobStatus.Pending;
            
            await _jobRepository.Save();

            if (job.Status == JobStatus.Active)
                await _recommendationService.UpdateJobEmbedding(job);

            return _mapper.Map<JobDto>(job);
        }
        public void DeleteJob(int jobId)
        {
            _jobRepository.Delete(jobId);
        }

        public async Task<bool> HideJobAsync(int jobId)
        {
            return await _jobRepository.HideJobAsync(jobId);
        }

        public async Task<bool> UnhideJobAsync(int jobId)
        {
            return await _jobRepository.UnhideJobAsync(jobId);
        }
        public async Task<(IEnumerable<GetListSearchJobDto> Jobs, int Total)> GetListSearch(JobSearchRequestDto request)
        {
            var (jobs, total) = await _jobRepository.GetListSearch(request);

            var mappedJobs = _mapper.Map<IEnumerable<GetListSearchJobDto>>(jobs);

            return (mappedJobs, total);
        }
        public async Task<ExpiredJobsResultDto> HideExpiredJobsAsync()
        {
            var expiredJobs = await _jobRepository.HideExpiredJobsAsync();

            var result = new ExpiredJobsResultDto
            {
                HiddenCount = expiredJobs.Count,
                HiddenJobs = _mapper.Map<List<ExpiredJobDto>>(expiredJobs)
            };

            return result;
        }

        public async Task<bool> ApproveJobAsync(int jobId)
        {
            throw new NotImplementedException();
        }
        //public async Task<bool> ApproveJobAsync(int jobId)
        //{
        //    return await _jobRepository.ApproveJobAsync(jobId);
        //}

        public async Task<IEnumerable<JobDto>> GetJobsByUserIdAsync(int userId)
        {
            var jobs = await _jobRepository.GetJobsByEmployerId(userId);
            return _mapper.Map<IEnumerable<JobDto>>(jobs);
        }
        public async Task<bool> CheckLimitCreateJob(int userID, int? maxJobsPerDay = null)
        {
            return await _jobRepository.CheckLimitCreateJob(userID, maxJobsPerDay);
        }
        public async Task<bool> CheckLimitCreateFeaturedJob(int userID)
        {
            return await _jobRepository.CheckLimitCreateFeaturedJob(userID);
        }
        //public async Task<bool> ToggleJobPriorityAsync(int jobId)
        //{
        //    var job = await _jobRepository.GetByJobId(jobId);
        //    if (job == null) return false;

        //    if (!job.Priority)
        //    {
        //        var hasAvailableFeaturedSlot = await _jobRepository.CheckLimitCreateFeaturedJob(job.UserID);
        //        if (!hasAvailableFeaturedSlot)
        //        {
        //            return false; 
        //        }
        //    }

        //    return await _jobRepository.ToggleJobPriorityAsync(jobId);
        //}
        public async Task<bool> ToggleJobPriorityAsync(int jobId)
        {
            var job = await _jobRepository.GetByJobId(jobId);
            if (job == null)
                return false;

            if (job.Status == JobStatus.Pending ||
                job.Status == JobStatus.Rejected ||
                (job.Deadline.HasValue && job.Deadline.Value < System.DateTime.Now))
            {
                return false;
            }

            return await _jobRepository.ToggleJobPriorityAsync(jobId);
        }
        public async Task<bool> IsDuplicateJobTitleAsync(int userID, string title)
        {
            if (string.IsNullOrWhiteSpace(title) || userID <= 0)
            {
                return false;
            }

            // Normalize the title for comparison (trim and convert to lowercase)
            string normalizedTitle = title.Trim().ToLower();

            // Check if a job with this title already exists for this user
            return await _jobRepository.IsDuplicateJobTitle(userID, normalizedTitle);
        }

        public async Task<bool> IsDuplicateJobTitleForUpdateAsync(int userID, int jobID, string title)
        {
            if (string.IsNullOrWhiteSpace(title) || userID <= 0 || jobID <= 0)
            {
                return false;
            }

            // Normalize the title for comparison
            string normalizedTitle = title.Trim().ToLower();

            // Check if any other job (excluding the current one) has the same title
            return await _jobRepository.IsDuplicateJobTitleForUpdate(userID, jobID, normalizedTitle);
        }
        public async Task<IEnumerable<object>> JobCategoryDashboard()
        {
            return await _jobRepository.JobCategoryDashboard();
        }
        public async Task<IEnumerable<object>> JobStatusDashboard()
        {
            return await _jobRepository.JobStatusDashboard();
        }
        public async Task<IEnumerable<object>> JobLocationDashboard()
        {
            return await _jobRepository.JobLocationDashboard();
        }
    }
}
