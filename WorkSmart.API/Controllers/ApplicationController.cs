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
                            string rejectionReason = !string.IsNullOrEmpty(request.RejectionReason) ? request.RejectionReason : "";

                            var emailContent = new Core.Dto.MailDtos.MailContent
                            {
                                To = candidate.User.Email,
                                Subject = "Your Application Status - Not Selected",
                                Body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Application Status Update</title>
</head>
<body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f9f9f9;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);"">
        <div style=""background-color: #f44336; color: white; padding: 20px; text-align: center;"">
            <h2 style=""margin: 0; padding: 0;"">Application Status Update</h2>
        </div>
        
        <div style=""padding: 30px;"">
            <h1 style=""color: #f44336; text-align: center; margin-top: 0;"">Application Not Selected</h1>
            
            <div style=""background-color: #fff1f0; border-left: 4px solid #f44336; padding: 15px; margin-bottom: 20px; border-radius: 4px;"">
                <p style=""margin-top: 0;"">Dear {candidate.User.FullName},</p>
                <p>We appreciate your interest in <strong>{jobTitle}</strong> at our company and the time you've taken to apply.</p>
                <p>After careful consideration, we regret to inform you that we have decided not to move forward with your application at this time.</p>
                {(string.IsNullOrEmpty(rejectionReason) ? "" : $"<p><strong>Reason:</strong> {rejectionReason}</p>")}
            </div>
            
            <p>Although we are unable to offer you this position, we encourage you to apply for future openings that match your skills and experience.</p>
            
            <p>Thank you again for your interest in our company. We wish you the best in your job search and professional endeavors.</p>
            
            <p style=""margin-top: 30px;"">Best regards,<br>WorkSmart Team</p>
        </div>
        
        <div style=""background-color: #f5f5f5; padding: 20px; text-align: center; font-size: 12px; color: #777;"">
            <p style=""margin: 0;"">© 2025 WorkSmart. All rights reserved.</p>
        </div>
    </div>
</body>
</html>"
                            };
                            await _sendMailService.SendMail(emailContent);
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

                            var emailContent = new Core.Dto.MailDtos.MailContent
                            {
                                To = candidate.User.Email,
                                Subject = "Your Application Status - Accepted",
                                Body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Application Status Update</title>
</head>
<body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f9f9f9;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);"">
        <div style=""background-color: #4CAF50; color: white; padding: 20px; text-align: center;"">
            <h2 style=""margin: 0; padding: 0;"">Application Status Update</h2>
        </div>
        
        <div style=""padding: 30px;"">
            <h1 style=""color: #4CAF50; text-align: center; margin-top: 0;"">Congratulations!</h1>
            
            <div style=""background-color: #f0fff0; border-left: 4px solid #4CAF50; padding: 15px; margin-bottom: 20px; border-radius: 4px;"">
                <p style=""margin-top: 0;"">Dear {candidate.User.FullName},</p>
                <p>We are pleased to inform you that your application for <strong>{jobTitle}</strong> has been accepted.</p>
            </div>
            
            <p>Our team was impressed with your qualifications and experience, and we believe you would be a valuable addition to our company.</p>
            
            <p>We will contact you shortly with more details about the next steps in the hiring process.</p>
            
            <p style=""margin-top: 30px;"">Best regards,<br>WorkSmart Team</p>
        </div>
        
        <div style=""background-color: #f5f5f5; padding: 20px; text-align: center; font-size: 12px; color: #777;"">
            <p style=""margin: 0;"">© 2025 WorkSmart. All rights reserved.</p>
        </div>
    </div>
</body>
</html>"
                            };
                            await _sendMailService.SendMail(emailContent);
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
                        var employerEmailContent = new Core.Dto.MailDtos.MailContent
                        {
                            To = jobDetail.User.Email,
                            Subject = "New Application Received",
                            Body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>New Application</title>
</head>
<body style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f9f9f9;"">
    <div style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);"">
        <div style=""background-color: #673AB7; color: white; padding: 20px; text-align: center;"">
            <h2 style=""margin: 0; padding: 0;"">New Application Alert</h2>
        </div>
        
        <div style=""padding: 30px;"">
            <h1 style=""color: #673AB7; text-align: center; margin-top: 0;"">New Candidate Application</h1>
            
            <div style=""background-color: #f5f0ff; border-left: 4px solid #673AB7; padding: 15px; margin-bottom: 20px; border-radius: 4px;"">
                <p style=""margin-top: 0;"">Dear {jobDetail.User.FullName},</p>
                <p>We are pleased to inform you that a new application has been received for the job <strong>{jobDetail.Title}</strong>.</p>
            </div>
            
            <p>You can review this application and all other candidates from your employer dashboard.</p>
            
            <div style=""text-align: center; margin: 30px 0;"">
                <a href=""#"" style=""display: inline-block; background-color: #673AB7; color: white; text-decoration: none; padding: 12px 24px; border-radius: 4px; font-weight: bold;"">View Applications</a>
            </div>
            
            <p style=""margin-top: 30px;"">Best regards,<br>WorkSmart Team</p>
        </div>
        
        <div style=""background-color: #f5f5f5; padding: 20px; text-align: center; font-size: 12px; color: #777;"">
            <p style=""margin: 0;"">© 2025 WorkSmart. All rights reserved.</p>
        </div>
    </div>
</body>
</html>"
                        };
                        await _sendMailService.SendMail(employerEmailContent);
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