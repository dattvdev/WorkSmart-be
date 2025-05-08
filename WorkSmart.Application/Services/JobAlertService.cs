using AutoMapper;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Helpers;
using WorkSmart.Core.Interface;
using WorkSmart.Repository.Repository;

namespace WorkSmart.Application.Services
{
    public class JobAlertService
    {
        private readonly IJobAlertRepository _jobAlertRepo;
        private readonly IMapper _mapper;

        public JobAlertService(IJobAlertRepository jobAlertRepo, IMapper mapper)
        {
            _jobAlertRepo = jobAlertRepo;
            _mapper = mapper;
        }

        public async Task<bool> CreateJobAlert(JobAlertCreateDto request)
        {
            var entity = _mapper.Map<JobAlert>(request);
            entity.CreatedAt = TimeHelper.GetVietnamTime();

            entity.JobPosition = null;
            entity.Frequency = null;

            await _jobAlertRepo.Add(entity);
            await _jobAlertRepo.Save();
            return true;
        }

        public async Task<List<JobAlertDto>> GetAlertsByUser(int userId)
        {
            var data = await _jobAlertRepo.GetJobAlertsByUserId(userId);
            var dtos = _mapper.Map<List<JobAlertDto>>(data);

            // Set Frequency to null for all returned DTOs
            foreach (var dto in dtos)
            {
                dto.JobPosition = null;
                dto.Frequency = null;
            }

            return dtos;
        }
        public async Task<bool> DeleteAlert(int alertId, int userId)
        {
            var alert = await _jobAlertRepo.GetById(alertId);
            if (alert == null || alert.UserId != userId)
                return false;

            _jobAlertRepo.Delete(alert.JobAlertId);
            await _jobAlertRepo.Save();

            return true;
        }

        public async Task<List<JobAlertDto>> GetAllJobAlertsAsync()
        {
            var allAlerts = await _jobAlertRepo.GetAll(); 
            var dtos = _mapper.Map<List<JobAlertDto>>(allAlerts);

            foreach (var dto in dtos)
            {
                dto.JobPosition = null;
                dto.Frequency = null;
            }

            return dtos;
        }

        public async Task<List<JobAlert>> GetJobAlertsByJobId(int jobId)
        {
            return await _jobAlertRepo.GetJobAlertsByJobId(jobId);
        }
    }
}
