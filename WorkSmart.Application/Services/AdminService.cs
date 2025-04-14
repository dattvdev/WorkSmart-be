using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.AdminDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;
using WorkSmart.Repository.Repository;

namespace WorkSmart.Application.Services
{
    public class AdminService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ISendMailService _sendMailService;
        //private readonly ISignalRService _signalRService;
        private readonly JobRecommendationService _recommendationService;
        public AdminService(IAccountRepository accountRepository, IJobRepository jobRepository, ISendMailService sendMailService, JobRecommendationService recommendationService)
        {
            _accountRepository = accountRepository;
            _jobRepository = jobRepository;
            _sendMailService = sendMailService;
            _recommendationService = recommendationService;
        }

        public async Task<List<GetListVerificationDto>> GetPendingVerifications()
        {
            var users = await _accountRepository.GetAll();
            var pendingVerifications = users
                .Where(u => u.Role == "Employer" &&
                            (u.TaxVerificationStatus == "Pending" || u.LicenseVerificationStatus == "Pending"))
                .Select(u => new GetListVerificationDto
                {
                    UserID = u.UserID,
                    CompanyName = u.CompanyName,
                    CompanyDescription = u.CompanyDescription,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Industry = u.Industry,
                    CompanySize = u.CompanySize,
                    // Xác thực thuế
                    TaxId = u.TaxId,
                    TaxVerificationStatus = u.TaxVerificationStatus,
                    TaxVerificationReason = u.TaxVerificationReason,
                    // Xác thực GPKD
                    BusinessLicenseImage = u.BusinessLicenseImage,
                    LicenseVerificationStatus = u.LicenseVerificationStatus,
                    LicenseVerificationReason = u.LicenseVerificationReason,
                    VerificationLevel = u.VerificationLevel
                })
                .ToList();
            return pendingVerifications;
        }

        // In AdminService.cs
        public async Task<bool> ApproveJobAsync(int jobId)
        {
            var result = await _jobRepository.ApproveJobAsync(jobId);

            if (result)
            {
                // Get job details to access employer information
                var job = await _jobRepository.GetById(jobId);
                var employer = await _accountRepository.GetById(job.UserID);
                await _recommendationService.CreateEmbeddingForJobIfApproved(job);
                // Send notification
                //await _signalRService.SendNotificationToUser(
                //    job.UserID,
                //    "Job Approved",
                //    $"Your job '{job.Title}' has been approved and is now visible to job seekers."
                //);

                // Send email
                var emailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = employer.Email,
                    Subject = "Job Approval Notification",
                    Body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Job Approval</title>
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
            background-color: #f1f8ff;
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
        .button {{
            display: inline-block;
            background-color: #4285f4;
            color: white;
            text-decoration: none;
            padding: 12px 24px;
            border-radius: 4px;
            font-weight: bold;
            margin-top: 15px;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h2>Job Posting System</h2>
        </div>
        
        <div class=""content"">
            <h1 style=""color: #4285f4; text-align: center;"">Job Approved</h1>
            
            <div class=""message"">
                <p>Dear {employer.FullName},</p>
                <p>We are pleased to inform you that your job posting <strong>{job.Title}</strong> has been approved by our administrators.</p>
                <p>Your job is now visible to all job seekers on our platform.</p>
            </div>
            
            <div style=""text-align: center;"">
                <a href=""/api/Job/{job.JobID}"" class=""button"">View Job</a>
            </div>
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

            return result;
        }

        public async Task<bool> RejectJobAsync(int jobId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                reason = "No reason provided";
            }

            var result = await _jobRepository.RejectJobAsync(jobId, reason);

            if (result)
            {
                // Get job details to access employer information
                var job = await _jobRepository.GetById(jobId);
                var employer = await _accountRepository.GetById(job.UserID);

                // Send notification
                //await _signalRService.SendNotificationToUser(
                //    job.UserID,
                //    "Job Rejected",
                //    $"Your job '{job.Title}' has been rejected. Please check your email for details."
                //);

                // Send email
                var emailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = employer.Email,
                    Subject = "Job Rejection Notification",
                    Body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Job Rejection</title>
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
            background-color: #e74c3c;
            color: white;
            padding: 20px;
            text-align: center;
        }}
        .content {{
            padding: 30px;
        }}
        .message {{
            background-color: #fef5f5;
            border-left: 4px solid #e74c3c;
            padding: 15px;
            margin-bottom: 20px;
            border-radius: 4px;
        }}
        .reason {{
            background-color: #f9f9f9;
            padding: 15px;
            border-radius: 4px;
            margin-top: 20px;
        }}
        .footer {{
            background-color: #f5f5f5;
            padding: 20px;
            text-align: center;
            font-size: 12px;
            color: #777;
        }}
        .button {{
            display: inline-block;
            background-color: #e74c3c;
            color: white;
            text-decoration: none;
            padding: 12px 24px;
            border-radius: 4px;
            font-weight: bold;
            margin-top: 15px;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h2>Job Posting System</h2>
        </div>
        
        <div class=""content"">
            <h1 style=""color: #e74c3c; text-align: center;"">Job Rejected</h1>
            
            <div class=""message"">
                <p>Dear {employer.FullName},</p>
                <p>We regret to inform you that your job posting <strong>{job.Title}</strong> has been rejected by our administrators.</p>
            </div>
            
            <div class=""reason"">
                <h3>Reason for Rejection:</h3>
                <p>{reason}</p>
                <p>Please review our job posting guidelines and consider submitting a revised version of your job posting.</p>
            </div>
            
            <div style=""text-align: center;"">
                <a href=""http://localhost:5173/employer/dashboard"" class=""button"">Go to Dashboard</a>
            </div>
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

            return result;
        }
    }
}