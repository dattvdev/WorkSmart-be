using AutoMapper;
using WorkSmart.Core.Dto.ReportDtos;
using WorkSmart.Core.Dto.AdminDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;
using WorkSmart.Core.Helpers;

namespace WorkSmart.Application.Services
{
    public class ReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IJobRepository _jobRepository;
        private readonly IMapper _mapper;

        public ReportService(IReportRepository reportRepository, ICandidateRepository candidateRepository, IJobRepository jobRepository, IMapper mapper)
        {
            _reportRepository = reportRepository;
            _candidateRepository = candidateRepository;
            _jobRepository = jobRepository;
            _mapper = mapper;
        }

        public async Task<bool> CreateJobReport(int senderId, CreateReportJobDto reportDto)
        {
            var sender = await _candidateRepository.GetById(senderId);
            if (sender == null)
                return false;

            var job = await _jobRepository.GetById(reportDto.JobId);
            if (job == null)
                return false;

            var reportPost = new ReportPost
            {
                SenderID = senderId,
                JobID = reportDto.JobId,
                Content = reportDto.Content,
                Status = "Pending",
                CreatedAt = TimeHelper.GetVietnamTime()
            };

            await _reportRepository.CreateReport(reportPost);
            return true;
        }

        public async Task<IEnumerable<ReportListDto>> GetReportsForAdmin()
        {
            var reports = await _reportRepository.GetReportsByAdmin();
            return _mapper.Map<IEnumerable<ReportListDto>>(reports);
        }

        public async Task<string> CheckReportStatus(int userId, int jobId)
        {
            var report = await _reportRepository.CheckReportStatus(userId, jobId);
            return report?.Status ?? "None";
        }

        public async Task<bool> UpdateReportStatus(int reportId, string status)
        {
            var report = await _reportRepository.GetById(reportId);
            if (report == null)
                return false;

            report.Status = status;
            report.CreatedAt = TimeHelper.GetVietnamTime();

            _reportRepository.Update(report);
            await _reportRepository.Save();

            return true;
        }
    }
}
