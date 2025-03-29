using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkSmart.API.SignalRService;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;
using WorkSmart.Core.Interface;

namespace WorkSmart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly JobService _jobService;
        private readonly ILogger<JobController> _logger;
        private readonly SignalRNotificationService _signalRService;
        private readonly ISendMailService _sendMailService;
        private readonly NotificationJobTagService _notificationJobTagService;
        public JobController(JobService jobService
            , ILogger<JobController> logger
            , SignalRNotificationService signalRService
            , ISendMailService sendMailService
            , NotificationJobTagService notificationJobTagService)
        {
            _jobService = jobService;
            _logger = logger;
            _signalRService = signalRService;
            _sendMailService = sendMailService;
            _notificationJobTagService = notificationJobTagService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto createJobDto)
        {
            if (createJobDto == null)
                return BadRequest(new { message = "Invalid job data." });
            try
            {
                await _jobService.CreateJobAsync(createJobDto);

                await _signalRService.SendNotificationToUser(
                       createJobDto.UserID,
                       "Job Notification",
                       $"Your Job \"{createJobDto.Title}\" Create Successfilly",
                       $"/employer/manage-jobs"
                   );
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating job: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while creating the job." });
            }
        }

        [HttpGet("getAllJob")]
        public async Task<IActionResult> GetAllJob()
        {
            try
            {
                var jobs = await _jobService.GetAllJobsAsync();
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error lits job: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while list the job." });
            }
        }

        [HttpGet("getAllJobManage")]
        public async Task<IActionResult> GetJobsForManagement([FromQuery] JobSearchRequestDto request)
        {
            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdValue))
            //{
            //    return Unauthorized();
            //}

            //if (!User.IsInRole("Employer"))
            //{
            //    return Forbid();
            //}
            var (jobs, total) = await _jobService.GetJobsForManagement(request);
            var totalPage = (int)Math.Ceiling((double)total / request.PageSize);
            var totalJob = total;
            return Ok(new { totalJob, totalPage, jobs });
        }

        [HttpDelete("delete/{jobId}")]
        public IActionResult DeleteJob(int jobId)
        {
            try
            {
                _jobService.DeleteJob(jobId);
                return Ok(new { message = "Job deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to delete job", error = ex.Message });
            }
        }

        /// Update an existing job post
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] UpdateJobDto updateJobDto)
        {
            if (updateJobDto == null)
                return BadRequest(new { message = "Invalid job data." });

            try
            {
                var updatedJob = await _jobService.UpdateJobAsync(id, updateJobDto);
                if (updatedJob == null)
                    return NotFound(new { message = "Job not found." });

                return Ok(updatedJob);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating job ID {JobID}: {Message}", id, ex.Message);
                return StatusCode(500, new { message = "An error occurred while updating the job." });
            }
        }

        /// Hide a job post
        [HttpPut("hide/{id}")]
        public async Task<IActionResult> HideJob(int id)
        {
            try
            {
                var success = await _jobService.HideJobAsync(id);
                if (!success)
                    return NotFound(new { message = "Job not found." });

                return Ok(new { message = "Job post has been hidden successfully.", jobId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error hiding job ID {JobID}: {Message}", id, ex.Message);
                return StatusCode(500, new { message = "An error occurred while hiding the job." });
            }
        }

        /// Unhide a job post
        [HttpPut("unhide/{id}")]
        public async Task<IActionResult> UnhideJob(int id)
        {
            try
            {
                var success = await _jobService.UnhideJobAsync(id);
                if (!success)
                    return NotFound(new { message = "Job not found." });

                return Ok(new { message = "Job post has been unhidden successfully.", jobId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error unhiding job ID {JobID}: {Message}", id, ex.Message);
                return StatusCode(500, new { message = "An error occurred while unhiding the job." });
            }
        }

        /// Get job details by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobById(int id)
        {
            try
            {
                var (job, similarJobs) = await _jobService.GetJobById(id);
                if (job == null)
                    return NotFound(new { message = "Job not found." });

                return Ok(new { job, similarJobs });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching job ID {JobID}: {Message}", id, ex.Message);
                return StatusCode(500, new { message = "An error occurred while retrieving the job." });
            }
        }

        [HttpGet("GetListSearch")]
        public async Task<IActionResult> GetListSearch
            ([FromQuery] JobSearchRequestDto request)
        {
            var (jobs, total) = await _jobService.GetListSearch(request);
            var totalPage = (int)Math.Ceiling((double)total / request.PageSize);
            var totalJob = total;
            return Ok(new { totalJob, totalPage, jobs });
        }

        ///// Get jobs by employer ID
        //[HttpGet("employer/{employerId}")]
        //public async Task<IActionResult> GetJobsByEmployerId(int employerId)
        //{
        //    try
        //    {
        //        var jobs = await _jobService.GetJobsByEmployerId(employerId);
        //        return Ok(jobs);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Error fetching jobs for employer ID {EmployerID}: {Message}", employerId, ex.Message);
        //        return StatusCode(500, new { message = "An error occurred while retrieving jobs." });
        //    }
        //}

        ///// Get jobs by status
        //[HttpGet("status/{status}")]
        //public async Task<IActionResult> GetJobsByStatus(JobStatus status)
        //{
        //    try
        //    {
        //        var jobs = await _jobService.GetJobsByStatus(status);
        //        return Ok(jobs);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Error fetching jobs with status {Status}: {Message}", status, ex.Message);
        //        return StatusCode(500, new { message = "An error occurred while retrieving jobs." });
        //    }
        //}
        [HttpPost("hide-expired")]
        public async Task<IActionResult> HideExpiredJobs()
        {
            try
            {
                var result = await _jobService.HideExpiredJobsAsync();
                return Ok(new
                {
                    Success = true,
                    Message = $"Đã ẩn {result.HiddenCount} job hết hạn",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error hiding expired jobs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi ẩn job hết hạn."
                });
            }
        }
        //aproved job
        [HttpPut("{jobId}/approve")]
        //[Authorize(Roles = "Admin")] 
        public async Task<IActionResult> ApproveJob(int jobId)
        {
            var result = await _jobService.ApproveJobAsync(jobId);

            if (!result)
            {
                return NotFound($"Job with ID {jobId} not found");
            }
            var job = await _jobService.GetJobById(jobId);
            List<int> listTagIds = job.Item1.Tags;
            var listUserId = _notificationJobTagService.GetNotiUserByListTagID(listTagIds);
            var subject = "There is a new job that you might be interested in";

            foreach (var userId in listUserId.Result)
            {
                string tagsHtml = string.Join("", userId.TagNames.Select(tag => $"<span class='tag'>{tag}</span>"));
                var body = $@"
                         <html>
                            <head>
                                <style>
                                    body {{
                                        font-family: 'Segoe UI', Roboto, Arial, sans-serif;
                                        background-color: #f7f9fc;
                                        margin: 0;
                                        padding: 0;
                                        color: #333;
                                    }}
                                    .email-container {{
                                        max-width: 600px;
                                        margin: 30px auto;
                                        background: #ffffff;
                                        padding: 0;
                                        border-radius: 12px;
                                        box-shadow: 0px 8px 20px rgba(0, 0, 0, 0.1);
                                        overflow: hidden;
                                    }}
                                    .header {{
                                        background: linear-gradient(135deg, #0062cc, #1e90ff);
                                        color: white;
                                        padding: 25px 20px;
                                        font-size: 22px;
                                        font-weight: bold;
                                        text-align: center;
                                        letter-spacing: 0.5px;
                                    }}
                                    .logo {{
                                        text-align: center;
                                        margin-top: -10px;
                                        margin-bottom: 15px;
                                    }}
                                    .logo img {{
                                        height: 40px;
                                    }}
                                    .content {{
                                        padding: 30px 25px;
                                        font-size: 16px;
                                        line-height: 1.6;
                                        color: #444;
                                    }}
                                    .job-title {{
                                        font-size: 20px;
                                        font-weight: bold;
                                        color: #0062cc;
                                        margin: 15px 0;
                                        padding-bottom: 10px;
                                        border-bottom: 1px solid #eaeaea;
                                    }}
                                    .job-info {{
                                        background-color: #f8f9fa;
                                        border-left: 4px solid #0062cc;
                                        padding: 15px;
                                        margin: 20px 0;
                                        border-radius: 0 6px 6px 0;
                                    }}
                                    .tags {{
                                        font-size: 14px;
                                        color: #555;
                                        margin: 15px 0;
                                        display: flex;
                                        flex-wrap: wrap;
                                    }}
                                    .tag {{
                                        background-color: #e6f2ff;
                                        color: #0062cc;
                                        padding: 5px 10px;
                                        border-radius: 50px;
                                        margin-right: 8px;
                                        margin-bottom: 8px;
                                        display: inline-block;
                                    }}
                                    .button-container {{
                                        text-align: center;
                                        margin: 30px 0 20px;
                                    }}
                                    .button {{
                                        display: inline-block;
                                        padding: 14px 30px;
                                        font-size: 16px;
                                        font-weight: bold;
                                        color: #fff;
                                        background-color: #28a745;
                                        text-decoration: none;
                                        border-radius: 50px;
                                        transition: all 0.3s ease;
                                        box-shadow: 0 4px 8px rgba(40, 167, 69, 0.2);
                                    }}
                                    .button:hover {{
                                        background-color: #218838;
                                        transform: translateY(-2px);
                                        box-shadow: 0 6px 12px rgba(40, 167, 69, 0.3);
                                    }}
                                    .footer {{
                                        background-color: #f8f9fa;
                                        padding: 20px;
                                        font-size: 13px;
                                        color: #777;
                                        text-align: center;
                                        border-top: 1px solid #eaeaea;
                                    }}
                                    .social-links {{
                                        margin: 15px 0;
                                    }}
                                    .social-links a {{
                                        display: inline-block;
                                        margin: 0 10px;
                                        color: #0062cc;
                                        text-decoration: none;
                                    }}
                                    @media only screen and (max-width: 600px) {{
                                        .email-container {{
                                            width: 100%;
                                            margin: 0;
                                            border-radius: 0;
                                        }}
                                        .content {{
                                            padding: 20px 15px;
                                        }}
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class='email-container'>
                                    <div class='header'>
                                        New Job Opportunity
                                    </div>
                                    <div class='content'>
                                        <p>Hello {userId.FullName},</p>
                                        <p>We have found a new job opportunity that matches your skills and interests.</p>
                
                                        <div class='job-info'>
                                            <p class='job-title'>{job.Item1.Title}</p>
                                            <p><strong>Company:</strong> {job.Item1.CompanyName}</p>
                                            <p><strong>Location:</strong> {job.Item1.Location}</p>
                                            <p><strong>Salary:</strong> {job.Item1.Salary}</p>
                                        </div>
                
                                        <p><strong>Related Tags:</strong></p>
                                        <div class='tags'>
                                            {tagsHtml}
                                        </div>
                
                                        <p>Click the button below to view job details and apply now:</p>
                
                                        <div class='button-container'>
                                            <a href='/job-list/{job.Item1.JobID}' class='button'>View Job Details</a>
                                        </div>
                
                                        <p>If you have any questions, please don't hesitate to contact us.</p>
                
                                        <p>Best regards,<br>
                                        Recruitment Team {job.Item1.CompanyName}</p>
                                    </div>
                                    <div class='footer'>
                                        <p>This is an automated email. Please do not reply.</p>
                                        <div class='social-links'>
                                            <a href='#'>Website</a> |
                                            <a href='#'>Facebook</a> |
                                            <a href='#'>LinkedIn</a>
                                        </div>
                                        <p>© {DateTime.Now.Year} {job.Item1.CompanyName}. All rights reserved.</p>
                                    </div>
                                </div>
                            </body>
                        </html>
                ";
          
                await _sendMailService.SendEmailAsync(userId.Email, subject, body);
            }
            return Ok(new { success = true, message = "Job approved successfully" });
        }
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<JobDto>>> GetJobsByUserId(int userId)
        {
            var jobs = await _jobService.GetJobsByUserIdAsync(userId);

            return Ok(jobs);
        }
        [HttpGet("checkLimitCreateJobPerDay/{userId}")]
        public async Task<bool> CheckLimitCreateJob(int userId)
        {
            var check = await _jobService.CheckLimitCreateJob(userId);

            return check;
        }
        [HttpGet("CheckLimitCreateFeaturedJob/{userId}")]
        public async Task<bool> CheckLimitCreateFeaturedJob(int userId)
        {
            var check = await _jobService.CheckLimitCreateFeaturedJob(userId);

            return check;
        }
        
        [HttpPut("toggle-priority/{id}")]
        public async Task<IActionResult> ToggleJobPriority(int id)
        {
            try
            {
                var success = await _jobService.ToggleJobPriorityAsync(id);
                if (!success)
                {
                    var job = await _jobService.GetJobById(id);
                    if (job.Item1 == null)
                        return NotFound(new { message = "Job not found." });
                    else
                        return BadRequest(new { message = "You have reached your featured job limit. Upgrade your subscription to create more featured jobs." });
                }

                return Ok(new { message = "Job priority updated successfully.", jobId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating job priority for ID {JobID}: {Message}", id, ex.Message);
                return StatusCode(500, new { message = "An error occurred while updating the job priority." });
            }
        }

        [HttpGet("job-category-dashboard")]
        public async Task<IActionResult> JobCategoryDashboard()
        {
            try
            {
                var result = await _jobService.JobCategoryDashboard();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching job category dashboard: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while fetching job category dashboard." });
            }
        }

        [HttpGet("job-status-dashboard")]
        public async Task<IActionResult> JobStatusDashboard()
        {
            try
            {
                var result = await _jobService.JobStatusDashboard();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching job status dashboard: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while fetching job status dashboard." });
            }
        }
        [HttpGet("job-location-dashboard")]
        public async Task<IActionResult> JobLocationDashboard()
        {
            try
            {
                var result = await _jobService.JobLocationDashboard();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching job location dashboard: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while fetching job location dashboard." });
            }
        }
    }
}
