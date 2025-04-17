using AutoMapper;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class JobAlertService
    {
        private readonly IJobAlertRepository _jobAlertRepo;
        private readonly IMapper _mapper;

        public JobAlertService(IJobAlertRepository jobAlertRepo,IMapper mapper)
        {
            _jobAlertRepo = jobAlertRepo;
            _mapper = mapper;
        }

        public async Task<bool> CreateJobAlert(JobAlertCreateDto request)
        {
            var entity = _mapper.Map<JobAlert>(request);
            entity.CreatedAt = DateTime.Now;

            await _jobAlertRepo.Add(entity);
            await _jobAlertRepo.Save();
            return true;
        }

        public async Task<List<JobAlertDto>> GetAlertsByUser(int userId)
        {
            var alerts = _jobAlertRepo.GetJobAlertsByUserId(userId); 
            return _mapper.Map<List<JobAlertDto>>(alerts);
        }
        public async Task<bool> DeleteAlert(int alertId,int userId)
        {
            var alert = await _jobAlertRepo.GetById(alertId);
            if (alert == null || alert.UserId != userId)
                return false;

            _jobAlertRepo.Delete(alert.JobAlertId); 
            await _jobAlertRepo.Save();

            return true;
        }


    }
}
