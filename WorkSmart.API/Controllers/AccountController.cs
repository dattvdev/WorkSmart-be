﻿using Microsoft.AspNetCore.Http;
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

                // Kiểm tra Role hợp lệ (Candidate, Employer)
                if (request.Role != "Candidate" && request.Role != "Employer")
                {
                    return BadRequest(new { Error = "Invalid role. Role must be 'Candidate' or 'Employer'." });
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
                    user.WorkLocation = request.WorkLocation;
                }

                await _accountRepository.Add(user);
                await _accountRepository.Save();

                var emailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = user.Email,
                    Subject = "Email Confirmation",
                    Body = $"<h1>Confirm Your Email</h1><p>Please use the following code to confirm your email address: <strong>{confirmationCode}</strong></p>"
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

                if (googleLoginRequest.Role != "Candidate" && googleLoginRequest.Role != "Employer")
                {
                    return BadRequest(new { Error = "Invalid role. Role must be 'Candidate' or 'Employer'." });
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
                    Body = $"<p>Your OTP code is: <b>{token}</b></p><p>The code is valid for 10 minutes.</p>"
                };
                await _sendMailService.SendMail(emailContent);

                return Ok("Password recovery email has been sent.");
            }
            catch (SqlException ex) when (ex.Number == -2) //Timeout error
            {
                return StatusCode(500, new { error = "Database connection timed out. Please try again." });
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
                var user = _accountRepository.GetByEmail(request.Email);
                if (user == null)
                {
                    return BadRequest("Email does not exist.");
                }

                if (!await _tokenRepository.ValidateOtpAsync(user.Email, request.Token))
                {
                    return BadRequest("OTP code is invalid or expired.");
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                _accountRepository.Update(user);
                await _accountRepository.Save();

                return Ok("Password has been reset successfully.");
            }
            catch (SqlException ex) when (ex.Number == -2) //Timeout error
            {
                return StatusCode(500, new { error = "Database connection timed out. Please try again." });
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
                    Body = "<p>Your password has been changed successfully.</p>"
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
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
