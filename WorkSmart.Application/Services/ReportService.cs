using AutoMapper;
using WorkSmart.Core.Dto.ReportDtos;
using WorkSmart.Core.Dto.AdminDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

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
            if (sender == null || sender.Role != "Candidate")
                return false;

            var job = await _jobRepository.GetById(reportDto.JobId);
            if (job == null)
                return false;

            var reportPost = new ReportPost
            {
                SenderID = senderId,
                JobID = reportDto.JobId,
                Title = reportDto.Title,
                Content = reportDto.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _reportRepository.CreateReport(reportPost);
            return true;
        }

        public async Task<(IEnumerable<ReportListDto> Reports, int Total)> GetReportsForAdmin(int pageNumber, int pageSize)
        {
            var (reports, total) = await _reportRepository.GetReportsByAdmin(pageNumber, pageSize);

            var reportListDtos = reports.Select(r => new ReportListDto
            {
                ReportPostID = r.ReportPostID,
                SenderID = r.SenderID,
                SenderName = r.Sender.FullName,
                SenderAvatar = r.Sender.Avatar,
                JobID = r.JobID,
                JobTitle = r.Job.Title,
                ReportTitle = r.Title,
                ReportContent = r.Content,
                CreatedAt = r.CreatedAt
            }).ToList();

            return (reportListDtos, total);
        }
    }
}
