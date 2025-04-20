using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WorkSmart.API.SignalRService;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.ApplicationDtos;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Dto.NotificationSettingDtos;
using WorkSmart.Core.Interface;
using static iTextSharp.text.pdf.qrcode.Version;

namespace WorkSmart.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly ApplicationService _applicationService;
        private readonly ISendMailService _sendMailService;
        private readonly SignalRNotificationService _signalRService;
        private readonly IJobRepository _jobRepository;
        private readonly NotificationSettingService _notificationSettingService;
        public ApplicationController(ApplicationService applicationService
            , ISendMailService sendMailService
            , SignalRNotificationService signalRService
            , IJobRepository jobRepository
            , NotificationSettingService notificationSettingService)
        {
            _applicationService = applicationService;
            _sendMailService = sendMailService;
            _signalRService = signalRService;
            _jobRepository = jobRepository;
            _notificationSettingService = notificationSettingService;
        }

        // Lấy danh sách ứng viên theo JobID
        [HttpGet("Job/{jobId}/applications")]
        public async Task<IActionResult> GetApplicationsByJobId(int jobId)
        {
            var applications = await _applicationService.GetApplicationsByJobIdAsync(jobId);

            if (applications == null || !applications.Any())
            {
                return NotFound(new { message = "No applications found for this job." });
            }

            return Ok(applications);
        }

        // Lấy danh sách job candidate đã apply
        [HttpGet("candidate/applied-jobs")]
        public async Task<IActionResult> GetApplicationsByUserId(int userId)
        {
            var applications = await _applicationService.GetApplicationsByUserIdAsync(userId);

            if (applications == null || !applications.Any())
            {
                return NotFound(new { message = "No applications found for this job." });
            }

            return Ok(applications);
        }

        // Cập nhật trạng thái ứng viên
        [HttpPut("update/{applicationId}")]
        public async Task<IActionResult> UpdateApplicationStatus(int applicationId, [FromBody] string status)
        {
            var result = await _applicationService.UpdateApplicationStatusAsync(applicationId, status);

            if (!result)
            {
                return NotFound(new { message = "Application not found." });
            }

            return Ok(new { message = "Application status updated successfully." });
        }

        // Từ chối ứng viên - Cập nhật để xử lý lý do từ chối
        [HttpPut("{candidateId}/reject")]
        public async Task<IActionResult> RejectCandidate(int candidateId, [FromBody] UpdateCandidateRequest request)
        {
            var candidate = await _applicationService.GetCandidateByIdAsync(candidateId);

            if (candidate == null)
            {
                return NotFound("Candidate not found.");
            }

            // Nếu đã bị từ chối trước đó, trả về thông báo
            if (candidate.Status == "Rejected")
            {
                return Ok("Candidate has been rejected before");
            }
            else
            {
                // Cập nhật trạng thái thành Rejected
                var result = await _applicationService.UpdateApplicationStatusAsync(candidateId, "Rejected");

                if (result)
                {
                    // Cập nhật lý do từ chối nếu được cung cấp
                    if (!string.IsNullOrEmpty(request.RejectionReason))
                    {
                        await _applicationService.UpdateRejectionReasonAsync(candidateId, request.RejectionReason);
                    }

                    // Cập nhật số lượng ứng viên bị từ chối
                    var rejectResult = await _applicationService.RejectCandidateAsync(candidateId);
                    if (!rejectResult)
                    {
                        return BadRequest("Failed to update recruitment number.");
                    }
                    CandidateNotificationSettingsDto candidateSetting = (CandidateNotificationSettingsDto)await _notificationSettingService.GetByIdAsync(candidate.UserID, candidate.User.Role); ;
                    //EmployerNotificationSettingsDto employerSetting = (EmployerNotificationSettingsDto)await _notificationSettingService.GetByIdAsync(candidate.Job.UserID, candidate.Job.User.Role);
                    // Lấy thông tin công việc cho email
                    var jobDetails = await _applicationService.GetJobDetailForApplicationAsync(candidateId);


                    if (candidateSetting != null)
                    {
                        if ((bool)candidateSetting.ApplicationRejected)
                        {
                            await _signalRService.SendNotificationToUser(
                                candidate.UserID,
                                "Application Status Updated",
                                $"We regret to inform you that your application \"{jobDetails.Title}\" has been rejected.",
                                "/candidate/applied-jobs"
                            );
                        }
                        if ((bool)candidateSetting.EmailApplicationRejected)
                        {
                            string jobTitle = jobDetails?.Title ?? "the position";

                            // Chuẩn bị nội dung email từ chối
                            string subject = "Your Application Status - Not Selected";
                            string body = $@"Dear {candidate.User.FullName},

                            We appreciate your interest in {jobTitle} at our company and the time you've taken to apply.

                            After careful consideration, we regret to inform you that we have decided not to move forward with your application at this time.";

                            // Thêm lý do từ chối vào email nếu có
                            if (!string.IsNullOrEmpty(request.RejectionReason))
                            {
                                body += $@"

                                Reason: {request.RejectionReason}";
                            }

                            body += $@"

                                Although we are unable to offer you this position, we encourage you to apply for future openings that match your skills and experience.

                                Thank you again for your interest in our company. We wish you the best in your job search and professional endeavors.

                                Best regards,
                                WorkSmart Team";

                            // Gửi email với lý do từ chối
                            await _sendMailService.SendEmailAsync(candidate.User.Email, subject, body);
                        }
                    }


                    return Ok(new
                    {
                        success = true,
                        message = "Candidate rejected successfully, recruitment number updated."
                    });
                }
            }

            return BadRequest("Failed to update application status.");
        }

        // Chấp nhận ứng viên
        [HttpPut("{candidateId}/accept")]
        public async Task<IActionResult> ApproveCandidate(int candidateId, [FromBody] UpdateCandidateRequest request)
        {
            var candidate = await _applicationService.GetCandidateByIdAsync(candidateId);

            if (candidate == null)
            {
                return NotFound("Candidate not found.");
            }
            if (candidate.Status == "Approved")
            {
                return Ok("Candidate has been approved before");
            }
            else
            {
                var result = await _applicationService.UpdateApplicationStatusAsync(candidateId, "Approved");

                if (result)
                {
                    var acceptResult = await _applicationService.AcceptCandidateAsync(candidateId);
                    if (!acceptResult)
                    {
                        return BadRequest("Failed to update recruitment number.");
                    }
                    CandidateNotificationSettingsDto candidateSetting = (CandidateNotificationSettingsDto)await _notificationSettingService.GetByIdAsync(candidate.UserID, candidate.User.Role); ;
                    //EmployerNotificationSettingsDto employerSetting = (EmployerNotificationSettingsDto)await _notificationSettingService.GetByIdAsync(candidate.Job.UserID, candidate.Job.User.Role);
                    // Lấy thông tin công việc cho email
                    var jobDetails = await _applicationService.GetJobDetailForApplicationAsync(candidateId);

                    if (candidateSetting != null)
                    {
                        if ((bool)candidateSetting.ApplicationApproved)
                        {
                            // Gửi thông báo realtime
                            await _signalRService.SendNotificationToUser(
                                candidate.UserID,
                                "Application Status Updated",
                                $"Congratulations! Your application \"{jobDetails.Title}\" has been approved.",
                                "/candidate/applied-jobs"
                            );
                        }
                        if ((bool)candidateSetting.EmailApplicationApproved)
                        {
                            string jobTitle = jobDetails?.Title ?? "the position";

                            string subject = "Your Application Status - Accepted";
                            string body = $@"Dear {candidate.User.FullName},

                            Congratulations! We are pleased to inform you that your application for {jobTitle} has been accepted.

                            Our team was impressed with your qualifications and experience, and we believe you would be a valuable addition to our company.

                            We will contact you shortly with more details about the next steps in the hiring process.

                            Best regards,
                            WorkSmart Team";

                            // Gửi email chấp nhận
                            await _sendMailService.SendEmailAsync(candidate.User.Email, subject, body);
                        }
                    }



                    /*    var employerId = jobDetails.UserID;
                           await _signalRService.SendNotificationToUser(
                               employerId,
                               "Candidate Acceptance Complete",
                               $"You've successfully accepted a candidate for \"{jobTitle}\".",
                               $"/employer/manage-jobs/applied-candidates/{jobDetails.JobID}"
                           );
                   */

                    return Ok("Candidate accepted successfully.");
                }
            }

            return BadRequest("Failed to update application status.");
        }

        [HttpPost("ApplyToJob")]
        public async Task<IActionResult> ApplyToJob(int userId, int jobId)
        {
            var (email, fullname) = await _applicationService.ApplyToJob(userId, jobId);
            if (email != null)
            {
                var jobDetail = await _jobRepository.GetById(jobId);
                CandidateNotificationSettingsDto candidateSetting = (CandidateNotificationSettingsDto)await _notificationSettingService.GetByIdAsync(userId, "candidate"); ;
                EmployerNotificationSettingsDto employerSetting = (EmployerNotificationSettingsDto)await _notificationSettingService.GetByIdAsync(jobDetail.UserID, "employer");
                await _sendMailService.SendEmailAsync(email, "Thanks for your application",
                $"Dear {fullname},\n\nYour application for the job has successfully.\n\nBest regards,\nYour Team");

                if (employerSetting != null)
                {
                    if ((bool)employerSetting.NewApplications)
                    {
                        await _signalRService.SendNotificationToUser(
                           jobDetail.UserID,
                           "New Application",
                           $"Your application for \"{jobDetail.Title}\" job has new Application.",
                           $"/employer/manage-jobs/applied-candidates/{jobDetail.JobID}"
                        );
                    }
                    if ((bool)employerSetting.EmailNewApplications)
                    {
                        // Gửi email thông báo có ứng viên mới
                        string body = $@"Dear {jobDetail.User.FullName},
                                         We are pleased to inform you that a new application has been received for the job {jobDetail.Title}. 
                                         Best regards,
                                         WorkSmart Team";
                        await _sendMailService.SendEmailAsync(jobDetail.User.Email
                            , "New Application Received"
                            , body);
                    }
                    if ((bool)candidateSetting.ApplicationApply)
                    {
                        await _signalRService.SendNotificationToUser(
                          userId,
                          "Application Notification",
                          $"Your Apply to \"{jobDetail.Title}\" has been applied",
                          $"/candidate/applied-jobs"
                      );
                    }
                    
                }
            }

            return Ok("Application submitted successfully.");
        }

        [HttpGet("Job/{jobId}/application/{applicationId}")]
        public async Task<ActionResult<ApplicationJobDto>> GetApplicationDetail(int jobId, int applicationId)
        {
            try
            {
                var applicationDetail = await _applicationService.GetApplicationDetailAsync(applicationId, jobId);
                return Ok(applicationDetail);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("CheckApplyStatus/{userId}/{jobId}")]
        public async Task<IActionResult> CheckApplyStatus(int userId, int jobId)
        {
            var result = await _applicationService.CheckApplyStatus(userId, jobId);
            return Ok(result);
        }

        [HttpGet("ApplicationCountDashboard")]
        public async Task<IActionResult> ApplicationCountDashboard()
        {
            var result = await _applicationService.ApplicationCountDashboard();
            return Ok(result);
        }

        [HttpGet("User/{userId}/applications/count")]
        public async Task<IActionResult> GetApplicationsCountByUserId(int userId)
        {
            var applicationsCount = await _applicationService.GetApplicationsCountByUserIdAsync(userId);
            return Ok(new { totalApplications = applicationsCount });
        }
    }
}