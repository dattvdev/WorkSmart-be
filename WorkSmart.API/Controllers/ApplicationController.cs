using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.ApplicationDtos;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Interface;

namespace WorkSmart.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly ApplicationService _applicationService;
        private readonly ISendMailService _sendMailService;  // Inject the SendMailService

        public ApplicationController(ApplicationService applicationService, ISendMailService sendMailService)
        {
            _applicationService = applicationService;
            _sendMailService = sendMailService;
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

        // Từ chối ứng viên
        [HttpPut("{candidateId}/reject")]
        public async Task<IActionResult> RejectCandidate(int candidateId, [FromBody] UpdateCandidateRequest request)
        {
             var candidate = await _applicationService.GetCandidateByIdAsync(candidateId);


            if (candidate == null)
            {
                return NotFound("Candidate not found.");
            }
            // kHi lỡ ấn rejected 2 lanaf thì vẫn sẽ không bị double +2 number of re....
            if (candidate.Status == "Rejected")
            {
                return Ok("Candidate has been rejected before");
            }
            else
            {
                var result = await _applicationService.UpdateApplicationStatusAsync(candidateId, "Rejected");
                if (result)
                {
                    var rejectResult = await _applicationService.RejectCandidateAsync(candidateId);
                    if (!rejectResult)
                    {
                        return BadRequest("Failed to update recruitment number.");
                    }

                    string subject = "Your Application Status - Rejected";
                    string body = $"Dear {candidate.User.FullName},\n\nWe regret to inform you that your application for the job has been rejected.\n\nBest regards,\nWorkSmart Team";

                    // Gửi email
                    await _sendMailService.SendEmailAsync(candidate.User.Email, subject, body);

                    return Ok("Candidate rejected successfully, recruitment number updated.");
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
            if(candidate.Status == "Approved")
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

                    await _sendMailService.SendEmailAsync(candidate.User.Email, "Your Application Status - Accepted",
                        $"Dear {candidate.User.FullName},\n\nYour application for the job has been approved.\n\nBest regards,\nYour Team");

                    return Ok("Candidate accepted successfully.");
                }
            }
            

            return BadRequest("Failed to update application status.");
        }


    }
}
