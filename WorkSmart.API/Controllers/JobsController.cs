using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
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
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly string filePath = "./freePlanSettings.json";
        private readonly CandidateService _candidateService;
        private readonly EmployerService _employerService;
        public JobController(JobService jobService
            , ILogger<JobController> logger
            , SignalRNotificationService signalRService
            , ISendMailService sendMailService
            , NotificationJobTagService notificationJobTagService
            , IServiceScopeFactory serviceScopeFactory // Thêm parameter này
            , CandidateService candidateService
            , EmployerService employerService)
        {
            _jobService = jobService;
            _logger = logger;
            _signalRService = signalRService;
            _sendMailService = sendMailService;
            _notificationJobTagService = notificationJobTagService;
            _serviceScopeFactory = serviceScopeFactory; // Gán giá trị
            _candidateService = candidateService;
            _employerService = employerService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto createJobDto)
        {
            if (createJobDto == null)
                return BadRequest(new { message = "Invalid job data." });
            try
            {
                bool canCreate = await _jobService.CheckLimitCreateJob(createJobDto.UserID, createJobDto.MaxJobsPerDay);

                if (!canCreate)
                {
                    return BadRequest(new { message = "Daily job creation limit reached" });
                }
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

        [HttpGet("getJobsActive")]
        public async Task<IActionResult> GetJobsActive()
        {
            try
            {
                var jobs = await _jobService.GetJobsActive();
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

        /// Get job details by ID (V2)
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetJobDetailsById(int id)
        {
            try
            {
                var (job, similarJobs) = await _jobService.GetJobById(id);
                if (job == null)
                    return NotFound(new { message = "Job not found." });

                return Ok(new { job });
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
                                    .button-container .button{{
                                        color: white;
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

        [HttpGet("checkLimitCreateJobPerDay/{userID}")]
        public async Task<bool> CheckLimitCreateJob(int userID, [FromQuery] int? maxJobsPerDay = null)
        {

            var check = await _jobService.CheckLimitCreateJob(userID, maxJobsPerDay);
            return check;
        }

        [HttpGet("getRemainingJobCreationLimit/{userID}")]
        public async Task<ActionResult<JobCreationLimitDto>> GetRemainingJobCreationLimit(int userID)
        {
            var result = await _jobService.GetRemainingJobCreationLimit(userID);
            return Ok(result);
        }

        [HttpGet("CheckLimitCreateFeaturedJob/{userId}")]
        public async Task<bool> CheckLimitCreateFeaturedJob(int userId)
        {
            var check = await _jobService.CheckLimitCreateFeaturedJob(userId);

            return check;
        }

        [HttpGet("getRemainingJobPriorityLimit/{userId}")]
        public async Task<ActionResult<JobPriorityLimitDto>> GetRemainingJobPriorityLimit(int userId)
        {
            var result = await _jobService.GetRemainingJobPriorityLimit(userId);

            return Ok(result);
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

        [HttpPut("un-priority-jobs/{jobId}")]
        public async Task<IActionResult> UnPriorityJob(int jobId)
        {
            try
            {
                var success = await _jobService.UnPriorityAsync(jobId);
                if (!success)
                {
                    var job = await _jobService.GetJobById(jobId);
                    if (job.Item1 == null)
                        return NotFound(new { message = "Job not found." });
                    else
                        return BadRequest(new { message = "Fail to unset job priority" });
                }

                return Ok(new { message = "Job priority updated successfully.", id = jobId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while unset the job priority." });
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

        private FreePlanSettings CreateDefaultSettings()
        {
            return new FreePlanSettings
            {
                employerFreePlan = new EmployerFreePlan
                {
                    MaxJobsPerDay = 1,
                    UpdatedAt = DateTime.Now.ToString(),
                    DefaultFeaturedJob = 0
                },
                candidateFreePlan = new CandidateFreePlan
                {
                    MaxCVsPerDay = 1,
                    UpdatedAt = DateTime.Now.ToString()
                }
            };
        }


        [HttpGet("FreePLanSettings")]
        public IActionResult GetFreePLanSettings()
        {
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    // Read the JSON file
                    var jsonData = System.IO.File.ReadAllText(filePath);

                    // If empty or invalid, create default
                    if (string.IsNullOrWhiteSpace(jsonData))
                    {
                        var defaultSettings = CreateDefaultSettings();
                        jsonData = JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);
                        System.IO.File.WriteAllText(filePath, jsonData);
                        return Ok(defaultSettings);
                    }

                    var settings = JsonConvert.DeserializeObject<FreePlanSettings>(jsonData);
                    return Ok(settings);
                }
                else
                {
                    // Create file with default settings
                    var defaultSettings = CreateDefaultSettings();
                    var jsonData = JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);
                    System.IO.File.WriteAllText(filePath, jsonData);
                    return Ok(defaultSettings);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error reading settings: {ex.Message}");
            }
        }


        [HttpPost("employerBasicPlan")]
        public IActionResult UpdateEmployerBasicPlan([FromBody] EmployerFreePlan employerFreePlan)
        {
            try
            {
                // Validate input
                if (employerFreePlan == null)
                {
                    return BadRequest("Invalid input");
                }

                // Add timestamp
                employerFreePlan.UpdatedAt = DateTime.Now.ToString();

                FreePlanSettings settings;
                if (System.IO.File.Exists(filePath))
                {
                    var jsonData = System.IO.File.ReadAllText(filePath);
                    settings = string.IsNullOrWhiteSpace(jsonData)
                        ? CreateDefaultSettings()
                        : JsonConvert.DeserializeObject<FreePlanSettings>(jsonData);
                }
                else
                {
                    settings = CreateDefaultSettings();
                }

                // Update employer settings
                settings.employerFreePlan = employerFreePlan;

                // Serialize to JSON and save to file
                var updatedJsonData = JsonConvert.SerializeObject(settings, Formatting.Indented);
                System.IO.File.WriteAllText(filePath, updatedJsonData);

                return Ok(employerFreePlan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating settings: {ex.Message}");
            }
        }

        [HttpPost("candidateBasicPlan")]
        public IActionResult UpdateCandidateBasicPlan([FromBody] CandidateFreePlan candidateFreePlan)
        {
            try
            {
                // Validate input
                if (candidateFreePlan == null)
                {
                    return BadRequest("Invalid input");
                }

                // Add timestamp
                candidateFreePlan.UpdatedAt = DateTime.Now.ToString();

                FreePlanSettings settings;
                if (System.IO.File.Exists(filePath))
                {
                    var jsonData = System.IO.File.ReadAllText(filePath);
                    settings = string.IsNullOrWhiteSpace(jsonData)
                        ? CreateDefaultSettings()
                        : JsonConvert.DeserializeObject<FreePlanSettings>(jsonData);
                }
                else
                {
                    settings = CreateDefaultSettings();
                }

                // Update candidate settings
                settings.candidateFreePlan = candidateFreePlan;

                // Serialize to JSON and save to file
                var updatedJsonData = JsonConvert.SerializeObject(settings, Formatting.Indented);
                System.IO.File.WriteAllText(filePath, updatedJsonData);

                return Ok(candidateFreePlan);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating settings: {ex.Message}");
            }
        }



        //[HttpGet("test-notifications")]
        //public async Task<IActionResult> TestJobNotifications()
        //{
        //    try
        //    {
        //        using var scope = _serviceScopeFactory.CreateScope();
        //        var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
        //        var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
        //        var sendMailService = scope.ServiceProvider.GetRequiredService<ISendMailService>();
        //        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        //        // Xử lý job sắp hết hạn (1-3 ngày)
        //        var expiringJobs = await jobRepository.GetExpiringJobsAsync();
        //        _logger.LogInformation("Found {Count} expiring jobs", expiringJobs.Count);

        //        foreach (var job in expiringJobs)
        //        {
        //            var today = DateTime.UtcNow.Date;
        //            int daysRemaining = 0;

        //            if (job.Deadline.HasValue)
        //            {
        //                daysRemaining = (job.Deadline.Value.Date - today).Days;
        //            }

        //            // Gửi thông báo trong ứng dụng
        //            await notificationService.CreateJobExpiringNotification(job, job.UserID, daysRemaining);

        //            // Gửi email
        //            var user = await userRepository.GetById(job.UserID);
        //            if (user != null && !string.IsNullOrEmpty(user.Email))
        //            {
        //                string subject = $"Job #{job.Title} sắp hết hạn";
        //                string body = $@"
        //            <html>
        //                <body>
        //                    <h2>Thông báo job sắp hết hạn</h2>
        //                    <p>Job <strong>{job.Title}</strong> của bạn sẽ hết hạn trong <strong>{daysRemaining} ngày</strong>.</p>
        //                    <p>Vui lòng xem xét và cập nhật thông tin nếu cần thiết.</p>
        //                </body>
        //            </html>";

        //                await sendMailService.SendEmailAsync(user.Email, subject, body);
        //                _logger.LogInformation("Sent test email to {Email} for expiring Job {JobID}", user.Email, job.JobID);
        //            }

        //            _logger.LogInformation("Created expiring notification for Job {JobID}", job.JobID);
        //        }

        //        // Xử lý job đã hết hạn 1 ngày
        //        var expiredJobs = await jobRepository.GetExpiredJobs();
        //        _logger.LogInformation("Found {Count} expired jobs", expiredJobs.Count);

        //        foreach (var job in expiredJobs)
        //        {
        //            // Gửi thông báo trong ứng dụng
        //            await notificationService.CreateJobExpiredNotification(job, job.UserID);

        //            // Gửi email
        //            var user = await userRepository.GetById(job.UserID);
        //            if (user != null && !string.IsNullOrEmpty(user.Email))
        //            {
        //                string subject = $"Job #{job.Title} đã hết hạn";
        //                string body = $@"
        //            <html>
        //                <body>
        //                    <h2>Thông báo job đã hết hạn</h2>
        //                    <p>Job <strong>{job.Title}</strong> của bạn đã hết hạn 1 ngày.</p>
        //                    <p>Nếu bạn muốn tiếp tục tuyển dụng, vui lòng cập nhật thông tin và gia hạn job.</p>
        //                </body>
        //            </html>";

        //                await sendMailService.SendEmailAsync(user.Email, subject, body);
        //                _logger.LogInformation("Sent test email to {Email} for expired Job {JobID}", user.Email, job.JobID);
        //            }

        //            _logger.LogInformation("Created expired notification for Job {JobID}", job.JobID);
        //        }

        //        return Ok(new
        //        {
        //            success = true,
        //            message = "Test completed successfully",
        //            expiringJobsCount = expiringJobs.Count,
        //            expiredJobsCount = expiredJobs.Count
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error testing job notifications");
        //        return StatusCode(500, new { success = false, message = ex.Message });
        //    }
        //}

        [HttpPost("send-invitation")]
        public async Task<IActionResult> SendJobInvitation([FromBody] JobInvitationRequestDto request)
        {
            if (request == null || request.CandidateId <= 0 || request.JobId <= 0)
            {
                return BadRequest("Invalid candidate or job information");
            }

            try
            {
                // Get job details
                var (jobResult, similarJobs) = await _jobService.GetJobById(request.JobId);
                if (jobResult == null)
                {
                    return NotFound($"Job with ID {request.JobId} not found");
                }

                // Get employer details
                var employer = await _employerService.GetEmployerProfile(request.EmployerId);
                if (employer == null)
                {
                    return NotFound($"Employer with ID {request.EmployerId} not found");
                }

                // Get candidate details
                var candidate = await _candidateService.GetCandidateProfile(request.CandidateId);
                if (candidate == null)
                {
                    return NotFound($"Candidate with ID {request.CandidateId} not found");
                }

                // Create email subject
                var subject = $"Job Opportunity: {jobResult.Title} at {employer.CompanyName}";

                // Create professional HTML email body
                var body = GenerateInvitationEmailBody(jobResult, candidate);

                // Send email
                await _sendMailService.SendEmailAsync(candidate.Email, subject, body);

                return Ok(new { success = true, message = "Invitation email sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to send invitation email", error = ex.Message });
            }
        }

        // Helper method to generate email body
        private string GenerateInvitationEmailBody(JobDetailDto job, GetCandidateProfileDto candidate)
        {
            string baseUrl = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["FrontendUrl:BaseUrl"];

            return $@"
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
                        background-color: #0062cc;
                        text-decoration: none;
                        border-radius: 50px;
                        transition: all 0.3s ease;
                    }}
                    .button:hover {{
                        background-color: #0051a8;
                    }}
                    .footer {{
                        background-color: #f8f9fa;
                        padding: 20px;
                        font-size: 13px;
                        color: #777;
                        text-align: center;
                        border-top: 1px solid #eaeaea;
                    }}
                </style>
            </head>
            <body>
                <div class='email-container'>
                    <div class='header'>
                        Job Opportunity at {job.CompanyName}
                    </div>
                    <div class='content'>
                        <p>Dear {candidate.FullName},</p>
                        
                        <p>I hope this email finds you well. We at {job.CompanyName} were impressed by your profile and qualifications, and we believe your skills and experience would be a valuable addition to our team.</p>
                        
                        <p>We would like to invite you to apply for the following position:</p>
                        
                        <div class='job-info'>
                            <p class='job-title'>{job.Title}</p>
                            <p><strong>Company:</strong> {job.CompanyName}</p>
                            <p><strong>Location:</strong> {job.Location}</p>
                            <p><strong>Salary Range:</strong> {job.Salary}</p>
                            <p><strong>Work Type:</strong> {job.WorkType}</p>
                        </div>
                        
                        <p>Your experience in particularly caught our attention, and we believe this opportunity aligns well with your career trajectory.</p>
                        
                        <p>If you are interested in exploring this opportunity further, please click the button below to respond to this invitation:</p>
                        
                        <div class='button-container'>
                            <a href='{baseUrl}/{job.JobID}' class='button'>View Job Details</a>
                        </div>
                        
                        <p>Should you have any questions or require additional information about the position or our company, please don't hesitate to contact us.</p>
                        
                        <p>We look forward to your response and potentially welcoming you to our team.</p>
                        
                        <p>Best regards,<br>
                        Recruitment Team<br>
                        {job.CompanyName}</p>
                    </div>
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} {job.CompanyName}. All rights reserved.</p>
                        <p>This email was sent to you because your profile matches our job requirements.</p>
                    </div>
                </div>
            </body>
        </html>
    ";
        }
    }
}
