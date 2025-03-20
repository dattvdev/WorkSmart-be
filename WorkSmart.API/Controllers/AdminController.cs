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

        public AdminController(IAccountRepository accountRepository, AdminService adminService, IMapper mapper, SendMailService sendMailService, SignalRNotificationService signalRService, IJobRepository jobRepository)
        {
            _accountRepository = accountRepository;
            _adminService = adminService;
            _mapper = mapper;
            _sendMailService = sendMailService;
            _signalRService = signalRService;
            _jobRepository = jobRepository;
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
                <p>Dear {{user.FullName}},</p>
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
                      "Your Verify Business License Has Been Rejected",
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
        // Add job approval endpoint
        [HttpPut("jobs/{jobId}/approve")]
        public async Task<IActionResult> ApproveJob(int jobId)
        {
            var result = await _adminService.ApproveJobAsync(jobId);

            if (!result)
            {
                return NotFound($"Job with ID {jobId} not found");
            }
            var jobDetail = await _jobRepository.GetById(jobId);
            await _signalRService.SendNotificationToUser(
                   jobDetail.UserID,
                   "Job Status Updated",
                   $"Congratulations! Your Job \"{jobDetail.Title}\" has been approved.",
                   "/employer/manage-jobs"
               );
            return Ok(new { success = true, message = "Job approved successfully" });
        }

        // Add job rejection endpoint
        [HttpPut("jobs/{jobId}/reject")]
        public async Task<IActionResult> RejectJob(int jobId, [FromBody] JobRejectionRequestDto request)
        {
            var result = await _adminService.RejectJobAsync(jobId, request.Reason);

            if (!result)
            {
                return NotFound($"Job with ID {jobId} not found");
            }
            var jobDetail = await _jobRepository.GetById(jobId);
            await _signalRService.SendNotificationToUser(
                   jobDetail.UserID,
                   "Job Status Updated",
                   $"We regret to inform you that your Job \"{jobDetail.Title}\" has been rejected.",
                   "/employer/manage-jobs"
                   );
            return Ok(new { success = true, message = "Job rejected successfully" });
        }
    }


}
