using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.ApplicationDtos;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;
using WorkSmart.Repository.Repository;

namespace WorkSmart.Application.Services
{
    public class ApplicationService
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IJobRepository _jobRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public ApplicationService(IApplicationRepository applicationRepository, IJobRepository jobRepository, IMapper mapper, IUserRepository userRepository)
        {
            _applicationRepository = applicationRepository;
            _jobRepository = jobRepository;
            _mapper = mapper;
            _userRepository = userRepository;
        }
        public async Task<Core.Entity.Application> GetCandidateByIdAsync(int candidateId)
        {
            var candidate = await _applicationRepository.GetCandidateByIdAsync(candidateId); // Sử dụng repository để lấy ứng viên
            return candidate;
        }

        // Lấy danh sách ứng viên đã ứng tuyển cho công việc (jobId)
        public async Task<IEnumerable<ApplicationJobDto>> GetApplicationsByJobIdAsync(int jobId)
        {
            // Lấy tất cả các ứng viên ứng tuyển cho theo jobId
            var applications = await _applicationRepository.GetApplicationsByJobIdAsync(jobId);

            var mapperApplicationDtos = _mapper.Map<IEnumerable<ApplicationJobDto>>(applications);

            return mapperApplicationDtos;
        }

        public async Task<IEnumerable<ApplicationJobDto>> GetApplicationsByUserIdAsync(int userId)
        {
            var applications = await _applicationRepository.GetApplicationsByUserIdAsync(userId);

            var mapperApplicationDtos = _mapper.Map<IEnumerable<ApplicationJobDto>>(applications);

            return mapperApplicationDtos;
        }

        // Cập nhật trạng thái ứng tuyển của ứng viên
        public async Task<bool> UpdateApplicationStatusAsync(int applicationId, string status)
        {
            return await _applicationRepository.UpdateApplicationStatusAsync(applicationId, status);
        }
        public async Task<bool> AcceptCandidateAsync(int candidateId)
        {
            var candidate = await GetCandidateByIdAsync(candidateId);
            if (candidate == null)
            {
                return false;
            }

                var job = await _jobRepository.GetById(candidate.JobID);
                if (job != null && job.NumberOfRecruitment > 0)
                {
                    job.NumberOfRecruitment -= 1;  // Taăng số lượng tuyển dụng
                    await _jobRepository.Save();
                }
     


            return true;
        }
        public async Task<bool> RejectCandidateAsync(int candidateId)
        {
            var candidate = await GetCandidateByIdAsync(candidateId);
            if (candidate == null)
            {
                return false;
            }
            //if(candidate.Status == "Rejected")
            //{
                
            //}
            //else
            //{
                var job = await _jobRepository.GetById(candidate.JobID);
                if (job != null && job.NumberOfRecruitment > 0)
                {
                    job.NumberOfRecruitment += 1;  // Taăng số lượng tuyển dụng
                    await _jobRepository.Save();
                }
            //}
            

            return true;
        }
        public async Task<ApplicationJobDto> GetApplicationDetailAsync(int applicationId, int jobId)
        {
            var application = await _applicationRepository.GetApplicationDetailAsync(applicationId, jobId);

            if (application == null)
            {
                throw new KeyNotFoundException($"Application with ID {applicationId} for job {jobId} not found.");
            }
            //Console.WriteLine($"Before mapping: User.Avatar = {application.User.Avatar}");
            var applicationDto = _mapper.Map<ApplicationJobDto>(application);
            //Console.WriteLine($"After mapping: DTO.Avatar = {applicationDto.Avatar}");
            //var applicationDto = _mapper.Map<ApplicationJobDto>(application);
            return applicationDto;
        }
        public async Task<(string email, string fullname)> ApplyToJob(int userId, int jobId)
        {
            await _applicationRepository.ApplyToJob(userId, jobId);
            var user = await _userRepository.GetById(userId);
            return (user.Email, user.FullName);
        }
        public async Task<bool> UpdateRejectionReasonAsync(int applicationId, string rejectionReason)
        {
            return await _applicationRepository.UpdateRejectionReasonAsync(applicationId, rejectionReason);
        }

        public async Task<Job> GetJobDetailForApplicationAsync(int applicationId)
        {
            return await _applicationRepository.GetJobDetailForApplicationAsync(applicationId);
        }

        public async Task<string> CheckApplyStatus(int userId, int jobId = 0)
        {
            if (jobId == 0)
            {
                return "Job ID is required.";
            }
            return  _applicationRepository.CheckApplyJob(userId, jobId);
        }

        public async Task<IEnumerable<object>> ApplicationCountDashboard()
        {
            var applicationCounts = await _applicationRepository.ApplicationCountDashboard();
            return applicationCounts;
        }

        public async Task<int> GetApplicationsCountByUserIdAsync(int userId)
        {
            var jobIds = await _jobRepository.GetJobIdsByUserIdAsync(userId);

            if (jobIds == null || !jobIds.Any())
            {
                return 0;
            }

            var applications = await _applicationRepository.GetApplicationsByJobIdsAsync(jobIds);

            return applications.Count();
        }
    }
}
