using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSmart.API.SignalRService;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.EmployerDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;
using WorkSmart.Repository.Repository;

namespace WorkSmart.API.Controllers
{
    [Route("employers")]
    [ApiController]
    public class EmployerController : ControllerBase
    {
        private readonly EmployerService _employerService;
        private readonly SignalRNotificationService _signalRService;
        private readonly SendMailService _sendMailService;
        private readonly IAccountRepository _accountRepository;

        public EmployerController(EmployerService employerService, SignalRNotificationService signalRService, SendMailService sendMailService, IAccountRepository accountRepository)
        {
            _employerService = employerService;
            _signalRService = signalRService;
            _sendMailService = sendMailService;
            _accountRepository = accountRepository;
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

        [HttpPut("edit-profile")]
        public async Task<IActionResult> EditEmployerProfile([FromBody] EditEmployerRequest request)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                //    return BadRequest(new { Error = "Invalid input data.", Details = ModelState });
                //}

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
    <title>Tax Verification</title>
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
            <h2>Tax Verification System</h2>
        </div>
        
        <div class=""content"">
            <h1 style=""color: #4285f4; text-align: center;"">Tax Verification Submitted Successfully</h1>
            
            <div class=""message"">
                <p>Dear Customer,</p>
                <p>We have received your tax verification request. Please wait for an administrator to approve your tax verification.</p>
            </div>
            
            <div style=""text-align: center;"">
                <a href=""{statusCheckUrl}"" class=""button"">Check Status</a>
            </div>
        </div>
        
        <div class=""footer"">
            <p>© 2025 Company Name. All rights reserved.</p>
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
                    Body = "<p>Please Wait For Admin To Approve Your Business License Verification</p>"
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
    }
}
