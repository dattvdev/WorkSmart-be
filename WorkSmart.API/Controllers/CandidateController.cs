using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSmart.API.SignalRService;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Dto.ReportDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;
using WorkSmart.Repository.Repository;

namespace WorkSmart.API.Controllers
{
    [Route("candidates")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private readonly CandidateService _candidateService;
        private readonly ReportService _reportService;
        private readonly IAccountRepository _accountRepository;
        private readonly SendMailService _sendMailService;
        private readonly SignalRNotificationService _signalRService;

        public CandidateController(CandidateService candidateService, ReportService reportService, IAccountRepository accountRepository, SendMailService sendMailService, SignalRNotificationService signalRService)
        {
            _candidateService = candidateService;
            _reportService = reportService;
            _accountRepository = accountRepository;
            _sendMailService = sendMailService;
            _signalRService = signalRService;
        }

        [HttpGet("GetListSearch")]
        public async Task<IActionResult> GetListSearchCandidate([FromQuery] CandidateSearchRequestDto request)
        {
            var (candidates, total) = await _candidateService.GetListSearchCandidate(request);
            var totalPage = (int)Math.Ceiling((double)total / request.PageSize);
            return Ok(new { totalPage, candidates });
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetCandidateProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null)
                {
                    return Unauthorized(new { Error = "UserId không tìm thấy trong token" });
                }

                var userId = int.Parse(userIdClaim.Value);

                var candidateProfile = await _candidateService.GetCandidateProfile(userId);

                if (candidateProfile == null)
                    return NotFound(new { Error = "Candidate not found." });

                return Ok(candidateProfile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while getting profile" });
            }
        }

        [HttpPut("edit-profile")]
        public async Task<IActionResult> EditCandidateProfile([FromBody] EditCandidateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Error = "Invalid input data.", Details = ModelState });
                }

                var userId = int.Parse(User.FindFirst("UserId")?.Value);

                var isUpdated = await _candidateService.EditCandidateProfile(userId, request);

                if (!isUpdated)
                    return NotFound(new { Error = "Candidate not found." });

                return Ok(new { Message = "Candidate profile updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while editing candidate profile" });
            }
        }

        [HttpPost("report-job")]
        public async Task<IActionResult> ReportJob([FromBody] CreateReportJobDto reportDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value);

                var user = await _accountRepository.GetById(userId);

                var result = await _reportService.CreateJobReport(userId, reportDto);

                if (!result)
                    return BadRequest(new { Error = "Unable to create report. Check job or user status." });

                await _signalRService.SendNotificationToUser(
                    userId,
                    "Job Report Submitted",
                    $"Your report has been received and is pending review."
                );

                var emailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = user.Email,
                    Subject = "Job Report Submitted - Pending Admin Review",
                    Body = $@"<!DOCTYPE html>
    <html lang=""en"">
    <head>
        <meta charset=""UTF-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        <title>Job Report Submitted</title>
        <style>
            body {{
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                line-height: 1.6;
                color: #333;
                margin: 0;
                padding: 0;
                background-color: #f9f9f9;
            }}
            .email-container {{
                max-width: 600px;
                margin: 0 auto;
                background-color: #ffffff;
                border-radius: 8px;
                overflow: hidden;
                box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
            }}
            .header {{
                background-color: #4285f4;
                color: white;
                padding: 20px;
                text-align: center;
            }}
            .content {{
                padding: 30px;
            }}
            .message {{
                background-color: #f0f8ff;
                border-left: 4px solid #4285f4;
                padding: 15px;
                margin-bottom: 20px;
                border-radius: 4px;
            }}
            .footer {{
                background-color: #f5f5f5;
                padding: 20px;
                text-align: center;
                font-size: 12px;
                color: #777;
            }}
        </style>
    </head>
    <body>
        <div class=""email-container"">
            <div class=""header"">
                <h2>Job Report Submitted</h2>
            </div>
            <div class=""content"">
                <h1 style=""color: #4285f4; text-align: center;"">Review in Progress</h1>
                <div class=""message"">
                    <p>Dear {user.FullName},</p>
                    <p>We want to confirm that your job report has been successfully submitted and is currently under review by our admin team. We appreciate your patience as we investigate the details of your report.</p>
                    <p>Our team will review the information carefully and take appropriate action. You will be notified of the outcome once the review is complete.</p>
                </div>
            </div>
            <div class=""footer"">
                <p>© 2025 WorkSmart. All rights reserved.</p>
                <p>Need help? Contact our support team.</p>
            </div>
        </div>
    </body>
    </html>
    "
                };

                await _sendMailService.SendMail(emailContent);

                return Ok(new { Message = "Job reported successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while reporting job" });
            }
        }
    }
}