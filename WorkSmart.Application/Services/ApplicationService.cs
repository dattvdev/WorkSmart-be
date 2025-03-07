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
        private readonly IMapper _mapper;
        public ApplicationService(IApplicationRepository applicationRepository, IJobRepository jobRepository, IMapper mapper)
        {
            _applicationRepository = applicationRepository;
            _jobRepository = jobRepository;
            _mapper = mapper;
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

        public async Task<IEnumerable<Core.Entity.Application>> GetApplicationsByUserIdAsync(int userId)
        {
            return await _applicationRepository.GetApplicationsByUserIdAsync(userId);
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

        public async Task ApplyToJob(int userId, int jobId, int cvId)
        {
            await _applicationRepository.ApplyToJob(userId, jobId, cvId);
        }
    }
}
