using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSmart.API.SignalRService;
using WorkSmart.Application.Mapper;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.AdminDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Interface;
using WorkSmart.Repository.Repository;

namespace WorkSmart.API.Controllers
{
    [Route("admins")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly AdminService _adminService;
        private readonly IMapper _mapper;
        private readonly SendMailService _sendMailService;
        private readonly SignalRNotificationService _signalRService;
        private readonly IJobRepository _jobRepository;
        private readonly IReportRepository _reportRepository;
        private readonly JobService _jobService;
        private readonly NotificationJobTagService _notificationJobTagService;
        private readonly ReportService _reportService;

        public AdminController(IAccountRepository accountRepository, AdminService adminService, IMapper mapper, SendMailService sendMailService, SignalRNotificationService signalRService, IJobRepository jobRepository, IReportRepository reportRepository, JobService jobService, NotificationJobTagService notificationJobTagService, ReportService reportService)
        {
            _accountRepository = accountRepository;
            _adminService = adminService;
            _mapper = mapper;
            _sendMailService = sendMailService;
            _signalRService = signalRService;
            _jobRepository = jobRepository;
            _reportRepository = reportRepository;
            _jobService = jobService;
            _notificationJobTagService = notificationJobTagService;
            _reportService = reportService;
        }

        [HttpGet("test-auth")]
        public IActionResult TestAuth()
        {
            var userRole = User.FindFirst("Role")?.Value;
            if (userRole == "Admin" || userRole == "Employer" || userRole == "Candidate")
            {
                return Ok($"Authorized with role: {userRole}");
            }
            return Unauthorized("Access denied");
        }

        [HttpGet("list-user")]
        public async Task<IActionResult> ViewListUser()
        {
            var users = await _accountRepository.GetAll();

            if (users == null || !users.Any())
            {
                return NotFound(new { Message = "No user to display" });
            }

            var filterUsers = users.Where(u => u.Role != "Admin").ToList();
            if (!filterUsers.Any())
            {
                return NotFound(new { Message = "No user to display after filtering" });
            }

            var result = _mapper.Map<List<AccountDto>>(filterUsers);

            return Ok(result);
        }

        [HttpGet("user-profile/{userId}")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            var user = await _accountRepository.GetById(userId);

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            var result = _mapper.Map<AccountDto>(user);
            return Ok(result);
        }

        [HttpPost("ban/{id}")]
        public async Task<IActionResult> BanUser(int id)
        {
            var user = await _accountRepository.GetById(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            if (user.IsBanned)
            {
                return BadRequest(new { Message = "This user is already banned" });
            }

            var emailContent = new Core.Dto.MailDtos.MailContent
            {
                To = user.Email,
                Subject = "Account Banned Notification",
                Body = $@"<!DOCTYPE html>
                            <html lang=""en"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Account Banned</title>
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
                                        background-color: #f8d7da;
                                        border-left: 4px solid #4285f4;
                                        padding: 15px;
                                        margin-bottom: 20px;
                                        border-radius: 4px;
                                        color: #721c24;
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
                                        <h2>Account Banned</h2>
                                    </div>
                                    <div class=""content"">
                                        <h1 style=""color: #4285f4; text-align: center;"">Your Account Has Been Banned</h1>
                                        <div class=""message"">
                                            <p>Dear {user.FullName},</p>
                                            <p>We regret to inform you that your account has been banned due to violations of our policies. If you believe this was a mistake, please contact our support team.</p>
                                        </div>
                                        <p style=""text-align: center;"">
                                            <a href=""#"" style=""display: inline-block; background-color: #4285f4; color: white; text-decoration: none; padding: 12px 24px; border-radius: 4px; font-weight: bold; text-align: center;"">Contact Support</a>
                                        </p>
                                    </div>
                                    <div class=""footer"">
                                        <p>© 2025 WorkSmart. All rights reserved.</p>
                                    </div>
                                </div>
                            </body>
                            </html>"
            };

            await _sendMailService.SendMail(emailContent);
            user.IsBanned = true;
            _accountRepository.Update(user);

            return Ok(new { Message = $"User {user.Email} has been banned successfully" });
        }

        [HttpPost("unban/{id}")]
        public async Task<IActionResult> UnbanUser(int id)
        {
            var user = await _accountRepository.GetById(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            if (!user.IsBanned)
            {
                return BadRequest(new { Message = "This user is not banned yet" });
            }

            await _signalRService.SendNotificationToUser(
                      id,
                      "Your account has been unbanned",
                      "Welcome back to our system, enjoy your stay"
            //$"/applications/{userId}/details"
            );

            var emailContent = new Core.Dto.MailDtos.MailContent
            {
                To = user.Email,
                Subject = "Account Unbanned Notification",
                Body = $@"<!DOCTYPE html>
                            <html lang=""en"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Account Unbanned</title>
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
                                        background-color: #e6ffed;
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
                                        <h2>Account Unbanned</h2>
                                    </div>
                                    <div class=""content"">
                                        <h1 style=""color: #4285f4; text-align: center;"">Welcome Back!</h1>
                                        <div class=""message"">
                                            <p>Dear {user.FullName},</p>
                                            <p>We are pleased to inform you that your account has been successfully unbanned. You may now log in and continue using our services.</p>
                                            <p>If you have any questions or concerns, please contact our support team.</p>
                                        </div>
                                        <p style=""text-align: center;"">
                                            <a href=""{{loginUrl}}"" style=""display: inline-block; background-color: #4285f4; color: white; text-decoration: none; padding: 12px 24px; border-radius: 4px; font-weight: bold; text-align: center;"">Log In</a>
                                        </p>
                                    </div>
                                    <div class=""footer"">
                                        <p>© 2025 WorkSmart. All rights reserved.</p>
                                    </div>
                                </div>
                            </body>
                            </html>"
            };

            await _sendMailService.SendMail(emailContent);

            user.IsBanned = false;
            _accountRepository.Update(user);

            return Ok(new { Message = $"User {user.Email} has been unbanned successfully" });
        }

        [HttpPost("approve-tax/{userId}")]
        public async Task<IActionResult> ApproveTax(int userId, [FromBody] ApproveVerificationRequest request)
        {
            var user = await _accountRepository.GetById(userId);
            if (user == null)
            {
                return NotFound("User not found or not an employer.");
            }

            if (user.TaxVerificationStatus != "Pending")
            {
                return BadRequest("Tax verification request is not in pending status.");
            }

            if (request.IsApproved)
            {
                user.TaxVerificationStatus = "Approved";
                user.VerificationLevel = 2; // Đã xác thực thuế
                user.TaxVerificationReason = null;

                await _signalRService.SendNotificationToUser(
                      userId,
                      "Your Verify Tax Has Been Approved",
                      "Please Verify Business License Before Post First Job"
                //$"/applications/{userId}/details"
                );

                var emailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = user.Email,
                    Subject = "Tax Verification Approved - Complete Your Business License Verification",
                    Body = $@"
                            <!DOCTYPE html>
                            <html lang=""en"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Tax Verification Approved</title>
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
                                        background-color: #e6ffed;
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
                                        <h2>Tax Verification Approved</h2>
                                    </div>
                                    <div class=""content"">
                                        <h1 style=""color: #4285f4; text-align: center;"">Congratulations!</h1>
                                        <div class=""message"">
                                            <p>Dear {user.FullName},</p>
                                            <p>We are pleased to inform you that your tax verification has been successfully approved.</p>
                                            <p>Before you can post your first job listing, please complete your business license verification.</p>
                                        </div>
                                        <p style=""text-align: center;"">
                                            <a href=""#"" style=""display: inline-block; background-color: #4285f4; color: white; text-decoration: none; padding: 12px 24px; border-radius: 4px; font-weight: bold; text-align: center;"">Verify Business License</a>
                                        </p>
                                    </div>
                                    <div class=""footer"">
                                        <p>© 2025 WorkSmart. All rights reserved.</p>
                                    </div>
                                </div>
                            </body>
                            </html>"
                };

                await _sendMailService.SendMail(emailContent);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return BadRequest("Rejection reason is required.");
                }
                user.TaxVerificationStatus = "Rejected";
                user.TaxVerificationReason = request.Reason;


                await _signalRService.SendNotificationToUser(
                      userId,
                      "Your Verify Tax Has Been Rejected",
                      "Please Verify Tax Again Before Post First Job"
                //$"/applications/{userId}/details"
                );

                var emailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = user.Email,
                    Subject = "Tax Verification Rejected - Please Resubmit Your Information",
                    Body = $@"
                            <!DOCTYPE html>
                            <html lang=""en"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Tax Verification Rejected</title>
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
                                        background-color: #dc3545;
                                        color: white;
                                        padding: 20px;
                                        text-align: center;
                                    }}
                                    .content {{
                                        padding: 30px;
                                    }}
                                    .message {{
                                        background-color: #ffe6e6;
                                        border-left: 4px solid #dc3545;
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
                                        <h2>Tax Verification Rejected</h2>
                                    </div>
                                    <div class=""content"">
                                        <h1 style=""color: #dc3545; text-align: center;"">Action Required!</h1>
                                        <div class=""message"">
                                            <p>Dear {user.FullName},</p>
                                            <p>We regret to inform you that your tax verification request has been rejected due to the following reason:</p>
                                            <p><strong>{user.TaxVerificationReason}</strong></p>
                                            <p>Please update your tax verification details and resubmit your request.</p>
                                        </div>
                                        <p style=""text-align: center;"">
                                            <a href=""#"" style=""display: inline-block; background-color: #dc3545; color: white; text-decoration: none; padding: 12px 24px; border-radius: 4px; font-weight: bold; text-align: center;"">Resubmit Tax Verification</a>
                                        </p>
                                    </div>
                                    <div class=""footer"">
                                        <p>© 2025 WorkSmart. All rights reserved.</p>
                                    </div>
                                </div>
                            </body>
                            </html>"
                };

                await _sendMailService.SendMail(emailContent);
            }

            user.UpdatedAt = DateTime.UtcNow;
            _accountRepository.Update(user);
            await _accountRepository.Save();

            return Ok(new { message = request.IsApproved ? "Tax verification approved." : "Tax verification rejected." });
        }

        [HttpPost("approve-license/{userId}")]
        public async Task<IActionResult> ApproveLicense(int userId, [FromBody] ApproveVerificationRequest request)
        {
            var user = await _accountRepository.GetById(userId);
            if (user == null)
            {
                return NotFound("User not found or not an employer.");
            }

            if (user.LicenseVerificationStatus != "Pending")
            {
                return BadRequest("Tax verification request is not in pending status.");
            }

            if (request.IsApproved)
            {
                user.LicenseVerificationStatus = "Approved";
                user.VerificationLevel = 3; // Đã xác thực giấy phép kinh doanh
                user.LicenseVerificationReason = null;


                await _signalRService.SendNotificationToUser(
                      userId,
                      "Your Verify Business License Has Been Approved",
                      "Let Create Your First Job Post"
                //$"/applications/{userId}/details"
                );

                var emailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = user.Email,
                    Subject = "Business License Verification Approved - Let Create Your Job Post",
                    Body = $@"
                            <!DOCTYPE html>
                            <html lang=""en"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Business License Verification Approved</title>
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
                                        background-color: #e6ffed;
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
                                        <h2>Business License Verification Approved</h2>
                                    </div>
                                    <div class=""content"">
                                        <h1 style=""color: #4285f4; text-align: center;"">Congratulations!</h1>
                                        <div class=""message"">
                                            <p>Dear {user.FullName},</p>
                                            <p>We are pleased to inform you that your business license verification has been successfully approved.</p>
                                            <p>You can now start posting job listings, viewing applications, and managing your recruitment process on our platform.</p>
                                        </div>
                                        <p style=""text-align: center;"">
                                            <a href=""{{#}}"" style=""display: inline-block; background-color: #4285f4; color: white; text-decoration: none; padding: 12px 24px; border-radius: 4px; font-weight: bold; text-align: center;"">Go to Dashboard</a>
                                        </p>
                                    </div>
                                    <div class=""footer"">
                                        <p>© 2025 WorkSmart. All rights reserved.</p>
                                    </div>
                                </div>
                            </body>
                            </html>"
                };

                await _sendMailService.SendMail(emailContent);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return BadRequest("Rejection reason is required.");
                }
                user.LicenseVerificationStatus = "Rejected";
                user.LicenseVerificationReason = request.Reason;

                await _signalRService.SendNotificationToUser(
                      userId,
                      "Your Verify Business License Has Been Rejected",
                      "Please Verify Business License Again Before Post First Job"
                //$"/applications/{userId}/details"
                );

                var emailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = user.Email,
                    Subject = "Business License Verification Rejected - Please Resubmit Your Information",
                    Body = $@"
                            <!DOCTYPE html>
                            <html lang=""en"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Business License Verification Rejected</title>
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
                                        background-color: #dc3545;
                                        color: white;
                                        padding: 20px;
                                        text-align: center;
                                    }}
                                    .content {{
                                        padding: 30px;
                                    }}
                                    .message {{
                                        background-color: #ffe6e6;
                                        border-left: 4px solid #dc3545;
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
                                        <h2>Business License Verification Rejected</h2>
                                    </div>
                                    <div class=""content"">
                                        <h1 style=""color: #dc3545; text-align: center;"">Action Required!</h1>
                                        <div class=""message"">
                                            <p>Dear {user.FullName},</p>
                                            <p>We regret to inform you that your business license verification request has been rejected due to the following reason:</p>
                                            <p><strong>{user.LicenseVerificationReason}</strong></p>
                                            <p>Please update your business license details and resubmit your request.</p>
                                        </div>
                                        <p style=""text-align: center;"">
                                            <a href=""{{#}}"" style=""display: inline-block; background-color: #dc3545; color: white; text-decoration: none; padding: 12px 24px; border-radius: 4px; font-weight: bold; text-align: center;"">Resubmit Business License Verification</a>
                                        </p>
                                    </div>
                                    <div class=""footer"">
                                        <p>© 2025 WorkSmart. All rights reserved.</p>
                                    </div>
                                </div>
                            </body>
                            </html>"
                };

                await _sendMailService.SendMail(emailContent);

            }

            //Gửi mail cho employer sau khi đã approve/reject license
            user.UpdatedAt = DateTime.UtcNow;
            _accountRepository.Update(user);
            await _accountRepository.Save();

            return Ok(new { message = request.IsApproved ? "Business license verification approved." : "Business license verification rejected." });
        }

        [HttpGet("pending-verifications")]
        public async Task<IActionResult> GetPendingVerifications()
        {
            var result = await _adminService.GetPendingVerifications();

            return Ok(result);
        }

        [HttpPut("jobs/{jobId}/approve")]
        public async Task<IActionResult> ApproveJob(int jobId)
        {
            var result = await _adminService.ApproveJobAsync(jobId);

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
                                            <a href='http://localhost:5173/job-list/{job.Item1.JobID}' class='button'>View Job Details</a>
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

            await _signalRService.SendNotificationToUser(
                   job.Item1.UserID,
                   "Job Status Updated",
                   $"Congratulations! Your Job \"{job.Item1.Title}\" has been approved.",
                   "/employer/manage-jobs"
               );
            return Ok(new { success = true, message = "Job approved successfully" });
        }

        [HttpPut("jobs/{jobId}/reject")]
        public async Task<IActionResult> RejectJob(int jobId, [FromBody] JobRejectionRequestDto request)
        {
            var result = await _adminService.RejectJobAsync(jobId, request.Reason);

            if (!result)
            {
                return NotFound($"Job with ID {jobId} not found");
            }
            var jobDetail = await _jobService.GetJobById(jobId);
            var listNotiTag = await _notificationJobTagService.GetNotiUserByListTagID(jobDetail.Item1.Tags);

            await _signalRService.SendNotificationToUser(
                   jobDetail.Item1.UserID,
                   "Job Status Updated",
                   $"We regret to inform you that your Job \"{jobDetail.Item1.Title}\" has been rejected.",
                   "/employer/manage-jobs"
                   );
            return Ok(new { success = true, message = "Job rejected successfully" });
        }

        [HttpPost("approve-report/{reportId}")]
        public async Task<IActionResult> ApproveReport(int reportId, [FromBody] ApproveReportRequest request)
        {
            var report = await _reportRepository.GetById(reportId);
            if (report == null)
            {
                return NotFound("Report not found.");
            }

            if (report.Status != "Pending")
            {
                return BadRequest("Report is not in pending status and cannot be processed.");
            }

            var sender = await _accountRepository.GetById(report.SenderID);
            if (sender == null)
            {
                return NotFound("Sender not found.");
            }

            if (request.IsApproved)
            {
                report.Status = "Completed";

                await _signalRService.SendNotificationToUser(
                    sender.UserID,
                    "Your Report Has Been Approved",
                    $"Your report has been approved."
                );

                var approvedEmailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = sender.Email,
                    Subject = "Your Report Has Been Approved",
                    Body = $@"
    <!DOCTYPE html>
    <html lang=""en"">
    <head>
        <meta charset=""UTF-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        <title>Report Approved</title>
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
                background-color: #e6ffed;
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
                <h2>Report Approved</h2>
            </div>
            <div class=""content"">
                <h1 style=""color: #4285f4; text-align: center;"">Approved</h1>
                <div class=""message"">
                    <p>Dear {sender.FullName},</p>
                    <p>We are pleased to inform you that your report for the job '{report.Content}' has been approved.</p>
                    <p>Our team has reviewed your report and found it to be valid and actionable.</p>
                </div>
                <p style=""text-align: center;"">
                    <a href=""#"" style=""display: inline-block; background-color: #4285f4; color: white; text-decoration: none; padding: 12px 24px; border-radius: 4px; font-weight: bold; text-align: center;"">View Report Details</a>
                </p>
            </div>
            <div class=""footer"">
                <p>© 2025 WorkSmart. All rights reserved.</p>
                <p>Questions? Contact our support team.</p>
            </div>
        </div>
    </body>
    </html>"
                };

                await _sendMailService.SendMail(approvedEmailContent);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return BadRequest("Rejection reason is required.");
                }

                report.Status = "Rejected";

                await _signalRService.SendNotificationToUser(
                     sender.UserID,
                    "Your Report Has Been Rejected",
                    $"Your report has been rejected."
                );

                var rejectedEmailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = sender.Email,
                    Subject = "Your Report Has Been Rejected",
                    Body = $@"
    <!DOCTYPE html>
    <html lang=""en"">
    <head>
        <meta charset=""UTF-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        <title>Report Rejected</title>
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
                background-color: #dc3545;
                color: white;
                padding: 20px;
                text-align: center;
            }}
            .content {{
                padding: 30px;
            }}
            .message {{
                background-color: #ffe6e6;
                border-left: 4px solid #dc3545;
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
                <h2>Report Rejected</h2>
            </div>
            <div class=""content"">
                <h1 style=""color: #dc3545; text-align: center;"">Action Required</h1>
                <div class=""message"">
                    <p>Dear {sender.FullName},</p>
                    <p>We regret to inform you that your report for the job '{report.Content}' has been rejected.</p>
                    <p>Reason for Rejection: <strong>{request.Reason}</strong></p>
                    <p>Please review the details and consider resubmitting with more information if applicable.</p>
                </div>
                <p style=""text-align: center;"">
                    <a href=""#"" style=""display: inline-block; background-color: #dc3545; color: white; text-decoration: none; padding: 12px 24px; border-radius: 4px; font-weight: bold; text-align: center;"">Resubmit Report</a>
                </p>
            </div>
            <div class=""footer"">
                <p>© 2025 WorkSmart. All rights reserved.</p>
                <p>Need help? Contact our support team.</p>
            </div>
        </div>
    </body>
    </html>"
                };

                await _sendMailService.SendMail(rejectedEmailContent);
            }

            report.CreatedAt = DateTime.UtcNow;
            _reportRepository.Update(report);
            await _reportRepository.Save();

            return Ok(new
            {
                message = request.IsApproved
                    ? "Report has been approved successfully."
                    : "Report has been rejected."
            });
        }

        [HttpGet("report-list")]
        public async Task<IActionResult> GetReportsForAdmin()
        {
            try
            {
                var reports = await _reportService.GetReportsForAdmin();
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while fetching reports" });
            }
        }
    }
}
