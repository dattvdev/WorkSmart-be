using AutoMapper;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using System.ComponentModel;
using System.Drawing;
using System.Reflection.Metadata;
using System.Security.Claims;
using WorkSmart.API.SignalRService;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.EmployerDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;
using WorkSmart.Repository.Repository;
using static System.Net.Mime.MediaTypeNames;

namespace WorkSmart.API.Controllers
{
    [Route("employers")]
    [ApiController]
    public class EmployerController : ControllerBase
    {
        private readonly EmployerService _employerService;
        private readonly UserService _userService;
        private readonly SignalRNotificationService _signalRService;
        private readonly SendMailService _sendMailService;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        public EmployerController(EmployerService employerService
            , SignalRNotificationService signalRService
            , SendMailService sendMailService
            , IAccountRepository accountRepository
            , IMapper mapper
            , UserService userService)
        {
            _employerService = employerService;
            _signalRService = signalRService;
            _sendMailService = sendMailService;
            _accountRepository = accountRepository;
            _mapper = mapper;
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetCompanyProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");

                var userId = int.Parse(userIdClaim.Value);

                var companyProfile = await _employerService.GetEmployerProfile(userId);

                if (companyProfile == null)
                    return NotFound(new { Error = "Employer not found" });

                return Ok(companyProfile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while getting profile.", Details = ex.Message });
            }
        }

        [HttpGet("company-profile/{companyId}")]
        public async Task<IActionResult> GetCompanyProfile(int companyId)
        {
            var company = await _accountRepository.GetById(companyId);

            if (company == null)
            {
                return NotFound(new { Message = "Company not found" });
            }

            var result = _mapper.Map<GetEmployerProfileDto>(company);
            return Ok(result);
        }


        [HttpPut("edit-profile")]
        public async Task<IActionResult> EditEmployerProfile([FromBody] EditEmployerRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Error = "Invalid input data.", Details = ModelState });
                }

                var userId = int.Parse(User.FindFirst("UserId")?.Value);
                
                var isUpdated = await _employerService.EditEmployerProfile(userId, request);

                if (!isUpdated)
                    return NotFound(new { Error = "Employer not found" });

                return Ok(new { Message = "Candidate profile updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while updating profile.", Details = ex.Message });
            }
        }

        [HttpPost("verify-tax")]
        public async Task<IActionResult> VerifyTax([FromBody] TaxVerificationDto taxVerificationDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value);

                var user = await _accountRepository.GetById(userId);

                var result = await _employerService.VerifyTax(userId, taxVerificationDto);


                await _signalRService.SendNotificationToUser(
                       userId,
                       "Send Verify Tax Success",
                       "Please Wait For Admin To Approve Your Tax Verification"
                //$"/applications/{userId}/details"
                );

                string statusCheckUrl = $"/applications/{userId}/details";
                var emailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = user.Email,
                    Subject = "Send Verify Tax Success",
                    Body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Tax Verification Submitted</title>
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
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h2>Tax Verification Submitted</h2>
        </div>
        <div class=""content"">
            <h1 style=""color: #4285f4; text-align: center;"">Verify Tax Submission</h1>
            <div class=""message"">
                <p>Dear {user.FullName},</p>
                <p>Your tax verification request has been successfully submitted. Please wait for the admin to review and approve your verification.</p>
                <p>You can check the status of your verification by clicking the link below:</p>
            </div>
            <p style=""text-align: center;"">
                <a href=""#"" style=""display: inline-block; background-color: #4285f4; color: white; text-decoration: none; padding: 12px 24px; border-radius: 4px; font-weight: bold; text-align: center;"">Check Status</a>
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
                
                return Ok("Tax verification submitted successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while verify tax.", Details = ex.Message });
            }
        }

        [HttpPost("upload-business-license")]
        public async Task<IActionResult> UploadBusinessLicense([FromBody] UploadBusinessLicenseDto request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value);

                var user = await _accountRepository.GetById(userId);

                if (string.IsNullOrEmpty(request.BusinessLicenseImageUrl))
                {
                    return BadRequest("Business license image URL is required.");
                }

                var result = await _employerService.UploadBusinessLicense(userId, request.BusinessLicenseImageUrl);

                if (!result)
                {
                    return NotFound("User not found");
                }

                await _signalRService.SendNotificationToUser(
                       userId,
                       "Send Verify Business License Success",
                       "Please Wait For Admin To Approve Your Business License Verification"
                //$"/applications/{userId}/details"
                );

                var emailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = user.Email,
                    Subject = "Send Verify Business License Success",
                    Body = $@"<!DOCTYPE html>
                            <html lang=""en"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Business License Verification</title>
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
                                </style>
                            </head>
                            <body>
                                <div class=""email-container"">
                                    <div class=""header"">
                                        <h2>Business License Verification Submitted</h2>
                                    </div>
                                    <div class=""content"">
                                        <h1 style=""color: #4285f4; text-align: center;"">Business License Verification</h1>
                                        <div class=""message"">
                                            <p>Dear {user.FullName},</p>
                                            <p>Your business license verification request has been successfully submitted. Our team will review your submission and update you on the approval status.</p>
                                            <p>You will receive a notification once your verification is approved.</p>
                                        </div>
                                    </div>
                                        <p style=""text-align: center;"">
                                            <a href=""#"" style=""display: inline-block; background-color: #4285f4; color: white; text-decoration: none; padding: 12px 24px; border-radius: 4px; font-weight: bold; text-align: center;"">Check Status</a>
                                        </p>
                                    <div class=""footer"">
                                        <p>© 2025 Your Company. All rights reserved.</p>
                                    </div>
                                </div>
                            </body>
                            </html>
                            "
                };
                await _sendMailService.SendMail(emailContent);

                return Ok(new { message = "Business license submitted for verification." });

            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while verify business license.", Details = ex.Message });
            }
        }
        [HttpGet("company-list/{companyName}")]
        public async Task<IActionResult> GetEmployerByCompanyName(string companyName)
        {
            var company = await _userService.GetEmployerByCompanyName(companyName);

            if (company == null)
            {
                return NotFound(new { Message = "Company not found" });
            }

            return Ok(company);
        }
    }
}
