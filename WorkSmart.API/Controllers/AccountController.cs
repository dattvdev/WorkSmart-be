using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.AccountDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;
using WorkSmart.Repository;
using WorkSmart.Repository.Repository;

namespace WorkSmart.API.Controllers
{
    [Route("accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly WorksmartDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly IAccountRepository _accountRepository;
        private readonly ISendMailService _sendMailService;
        private readonly ITokenRepository _tokenRepository;
        private readonly IDistributedCache _cache;

        public AccountController(WorksmartDBContext context, IConfiguration configuration, IAccountRepository accountRepository, ISendMailService sendMailService, ITokenRepository tokenRepository, IDistributedCache cache)
        {
            _context = context;
            _configuration = configuration;
            _accountRepository = accountRepository;
            _sendMailService = sendMailService;
            _tokenRepository = tokenRepository;
            _cache = cache;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState); 
                }

                var existingUser = _accountRepository.GetByEmail(request.Email);
                if (existingUser != null)
                {
                    if (!existingUser.IsEmailConfirmed)
                    {
                        return BadRequest(new { Error = "Verification code has been sent to your email, please verify your account." });
                    }

                    return BadRequest(new { Error = "The email address is already in use. Please choose a different one." });
                }

                var random = new Random();
                var confirmationCode = random.Next(100000, 999999).ToString();

                var user = new User
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    Role = request.Role,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    ConfirmationCode = confirmationCode,
                    IsEmailConfirmed = false,
                    Avatar = request.Avatar,
                    CreatedAt = DateTime.Now,
                };

                if(request.Role == "Employer")
                {
                    user.PhoneNumber = request.PhoneNumber;
                    user.Gender = request.Gender;
                    user.CompanyName = request.CompanyName;
                    user.Address = request.Address;
                }

                await _accountRepository.Add(user);
                await _accountRepository.Save();

                var emailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = user.Email,
                    Subject = "Email Confirmation",
                    Body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Email Confirmation</title>
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
        .verification-code {{
            font-size: 32px;
            letter-spacing: 5px;
            text-align: center;
            margin: 30px 0;
            color: #4285f4;
            font-weight: bold;
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
            <h2>Account Registration</h2>
        </div>
        
        <div class=""content"">
            <h1 style=""color: #4285f4; text-align: center;"">Confirm Your Email</h1>
            
            <div class=""message"">
                <p>Dear {user.FullName},</p>
                <p>Thank you for registering with our service. To complete your registration, please use the verification code below:</p>
            </div>
            
            <div class=""verification-code"">
                {confirmationCode}
            </div>
            
            <p style=""text-align: center;"">This code will expire in 30 minutes.</p>
            
            <p style=""margin-top: 30px; font-size: 14px; color: #777;"">If you did not request this verification, please ignore this email or contact our support team if you have any concerns.</p>
        </div>
        
        <div class=""footer"">
            <p>© 2025 WorkSmart. All rights reserved.</p>
        </div>
    </div>
</body>
</html>"
                };
                await _sendMailService.SendMail(emailContent);

                var token = GenerateJwtToken(user);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while processing your request.", ex.Message });
            }
        }

        [HttpPost("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            try
            {
                var user = _accountRepository.GetByEmail(request.Email);
                if (user == null || user.ConfirmationCode != request.Code)
                {
                    return BadRequest(new { Error = "Invalid confirmation code or email address." });
                }

                user.IsEmailConfirmed = true;
                user.ConfirmationCode = null;
                _accountRepository.Update(user);
                await _accountRepository.Save();

                return Ok(new { Message = "Email confirmed successfully. You can now log in." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while processing your request." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = _accountRepository.GetByEmail(request.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return Unauthorized(new { Error = "Invalid email or password. Please try again" });
                }

                if (!user.IsEmailConfirmed)
                {
                    return Unauthorized(new { Error = "Email not confirmed. Please check your email for the confirmation code." });
                }

                if (user.IsBanned)
                {
                    return Unauthorized(new { Error = "Your account is banned. Contact fanpage to get more information" });
                }

//                var emailContent = new Core.Dto.MailDtos.MailContent
//                {
//                    To = user.Email,
//                    Subject = "Email Confirmation",
//                    Body = $@"<!DOCTYPE html>
//<html lang=""en"">
//<head>
//    <meta charset=""UTF-8"">
//    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
//    <title>Welcome to WorkSmart</title>
//    <style>
//        body {{
//            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
//            line-height: 1.6;
//            color: #333;
//            margin: 0;
//            padding: 0;
//            background-color: #f9f9f9;
//        }}
//        .email-container {{
//            max-width: 600px;
//            margin: 0 auto;
//            background-color: #ffffff;
//            border-radius: 8px;
//            overflow: hidden;
//            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
//        }}
//        .header {{
//            background-color: #4285f4;
//            color: white;
//            padding: 20px;
//            text-align: center;
//        }}
//        .content {{
//            padding: 30px;
//        }}
//        .message {{
//            background-color: #f1f8ff;
//            border-left: 4px solid #4285f4;
//            padding: 15px;
//            margin-bottom: 20px;
//            border-radius: 4px;
//        }}
//        .footer {{
//            background-color: #f5f5f5;
//            padding: 20px;
//            text-align: center;
//            font-size: 12px;
//            color: #777;
//        }}
//        .button {{
//            display: inline-block;
//            background-color: #4285f4;
//            color: white;
//            text-decoration: none;
//            padding: 12px 24px;
//            border-radius: 4px;
//            font-weight: bold;
//            margin-top: 15px;
//            text-align: center;
//        }}
//    </style>
//</head>
//<body>
//    <div class=""email-container"">
//        <div class=""header"">
//            <h2>Welcome to WorkSmart</h2>
//        </div>
        
//        <div class=""content"">
//            <h1 style=""color: #4285f4; text-align: center;"">Hello, {user.FullName}!</h1>
            
//            <div class=""message"">
//                <p>We are thrilled to have you on board. Thank you for choosing WorkSmart!</p>
//                <p>Here are some things you can do now:</p>
//                <ul>
//                    <li>Complete your profile to get the best experience.</li>
//                    <li>Explore our features and start working smart.</li>
//                    <li>Join our community and connect with like-minded professionals.</li>
//                </ul>
//            </div>
            
//            <p style=""text-align: center;""><a href=""#"" class=""button"">Go to Dashboard</a></p>
            
//            <p style=""margin-top: 30px; font-size: 14px; color: #777;"">If you have any questions, feel free to contact our support team. We are here to help!</p>
//        </div>
        
//        <div class=""footer"">
//            <p>© 2025 WorkSmart. All rights reserved.</p>
//        </div>
//    </div>
//</body>
//</html>
//"
//                };

//                await _sendMailService.SendMail(emailContent);
                var token = GenerateJwtToken(user);
                return Ok(new
                {
                    Token = token,
                    User = new
                    {
                        user.UserID,
                        user.Email,
                        user.FullName,
                        user.Avatar,
                        user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest googleLoginRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(googleLoginRequest.Email))
                {
                    return BadRequest(new { Message = "Invalid Google login data" });
                }

                var user =  _accountRepository.GetByEmail(googleLoginRequest.Email);
                if(user == null)
                {
                    user = new User
                    {
                        FullName = googleLoginRequest.Name,
                        Email = googleLoginRequest.Email,
                        Avatar = googleLoginRequest.Avatar,
                        Role = googleLoginRequest.Role,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(googleLoginRequest.Email),
                        IsEmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                    };

                    if(googleLoginRequest.Role == "Employer")
                    {
                        user.PhoneNumber = "";
                        user.Gender = "";
                        user.CompanyName = "";
                        user.WorkLocation = "";
                    }

                    await _accountRepository.Add(user);
                    await _accountRepository.Save();
                }

                if (user.IsBanned)
                {
                    return Unauthorized(new { Message = "Your account is banned. Contact fanpage to get more information"});
                }

                var emailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = user.Email,
                    Subject = "Email Confirmation",
                    Body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Welcome to WorkSmart</title>
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
            <h2>Welcome to WorkSmart</h2>
        </div>
        
        <div class=""content"">
            <h1 style=""color: #4285f4; text-align: center;"">Hello, {user.FullName}!</h1>
            
            <div class=""message"">
                <p>We are thrilled to have you on board. Thank you for choosing WorkSmart!</p>
                <p>Here are some things you can do now:</p>
                <ul>
                    <li>Complete your profile to get the best experience.</li>
                    <li>Explore our features and start working smart.</li>
                    <li>Join our community and connect with like-minded professionals.</li>
                </ul>
            </div>
            
            <p style=""text-align: center;""><a href=""#"" class=""button"">Go to Dashboard</a></p>
            
            <p style=""margin-top: 30px; font-size: 14px; color: #777;"">If you have any questions, feel free to contact our support team. We are here to help!</p>
        </div>
        
        <div class=""footer"">
            <p>© 2025 WorkSmart. All rights reserved.</p>
        </div>
    </div>
</body>
</html>
"
                };

                await _sendMailService.SendMail(emailContent);

                var token = GenerateJwtToken(user);
                return Ok(new
                {
                    Token = token,
                    User = new
                    {
                        user.UserID,
                        user.Email,
                        user.FullName,
                        user.Avatar,
                        user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while processing your request." });
            }
        }

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var user = _accountRepository.GetByEmail(request.Email);
                if (user == null)
                {
                    return BadRequest("Email does not exist.");
                }

                var token = new Random().Next(100000, 999999).ToString();

                await _tokenRepository.SaveOtpAsync(user.Email, token);

                var emailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = user.Email,
                    Subject = "OTP code to reset password",
                    Body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Password Reset Request</title>
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
            background-color: #f2dede;
            border-left: 4px solid #4285f4;
            padding: 15px;
            margin-bottom: 20px;
            border-radius: 4px;
        }}
        .otp-code {{
            font-size: 32px;
            letter-spacing: 5px;
            text-align: center;
            margin: 30px 0;
            color: #4285f4;
            font-weight: bold;
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
            <h2>Password Reset Request</h2>
        </div>
        <div class=""content"">
            <h1 style=""color: #4285f4; text-align: center;"">Reset Your Password</h1>
            <div class=""message"">
                <p>Hello,</p>
                <p>You recently requested to reset your password. Please use the OTP code below to proceed:</p>
            </div>
            <div class=""otp-code"">
                {token}
            </div>
            <p style=""text-align: center;"">This code will expire in 10 minutes.</p>
            <p style=""margin-top: 30px; font-size: 14px; color: #777;"">If you did not request a password reset, please ignore this email or contact our support team.</p>
        </div>
        <div class=""footer"">
            <p>© 2025 WorkSmart. All rights reserved.</p>
        </div>
    </div>
</body>
</html>
"
                };
                await _sendMailService.SendMail(emailContent);

                return Ok("Password recovery email has been sent.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("verifyOTP")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOTPRequest request)
        {
            try
            {
                var user = _accountRepository.GetByEmail(request.Email);
                if (user == null)
                {
                    return BadRequest("Email does not exist.");
                }

                if (!await _tokenRepository.ValidateOtpAsync(request.Email, request.Otp))
                {
                    return BadRequest("OTP code is invalid or expired.");
                }

                // Tạo token xác thực reset password
                var resetToken = Guid.NewGuid().ToString();
                await _tokenRepository.SaveResetTokenAsync(request.Email, resetToken, TimeSpan.FromMinutes(15));

                return Ok(new { resetToken = resetToken });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = _accountRepository.GetByEmail(request.Email);
                if (user == null)
                {
                    return BadRequest("Email does not exist.");
                }

                if (!await _tokenRepository.ValidateResetTokenAsync(user.Email, request.ResetToken))
                {
                    return BadRequest("Reset token is invalid or expired.");
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                _accountRepository.Update(user);
                await _accountRepository.Save();

                await _tokenRepository.RemoveResetTokenAsync(user.Email);

                return Ok("Password has been reset successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPatch("changePassword")]
        public async Task<IActionResult> ChangePass([FromBody] ChangePasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = int.Parse(User.FindFirst("UserId")?.Value);

                var user = await _accountRepository.GetById(userId);
                if (user == null)
                {
                    return NotFound(new { Error = "User not found" });
                }

                if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
                {
                    return BadRequest(new { Error = "Old password is incorrect." });
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                _accountRepository.Update(user);
                await _accountRepository.Save();

                var emailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = user.Email,
                    Subject = "Password Changed Successfully",
                    Body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Password Changed Successfully</title>
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
            background-color: #e9f7ef;
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
            <h2>Password Change Confirmation</h2>
        </div>
        <div class=""content"">
            <h1 style=""color: #4285f4; text-align: center;"">Your Password Has Been Changed</h1>
            <div class=""message"">
                <p>Dear {user.FullName},</p>
                <p>We wanted to inform you that your password has been changed successfully. If you made this change, you can ignore this email.</p>
            </div>
            <p style=""text-align: center;"">If you did not request this change, please <a href=""#"" style=""color: #4285f4; font-weight: bold;"">contact support</a> immediately.</p>
            <p style=""margin-top: 30px; font-size: 14px; color: #777;"">For your security, if you did not request this change, we recommend updating your password as soon as possible.</p>
        </div>
        <div class=""footer"">
            <p>© 2025 WorkSmart. All rights reserved.</p>
        </div>
    </div>
</body>
</html>
"
                };
                await _sendMailService.SendMail(emailContent);
                return Ok("Password Changed Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok();
        }

        private object GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("UserId", user.UserID.ToString()),
            new Claim("Purpose", "purpose"),
            new Claim("Role", user.Role),
            new Claim("VerificationLevel", user.VerificationLevel.ToString()),
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
