using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                    {
                        return BadRequest(new { Error = "The email address is already in use. Please choose a different one." });
                    }
                }

                return StatusCode(500, new { Error = "An error occurred while processing your request." });
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
                    return BadRequest("Email không tồn tại.");
                }
                var token = GenerateJwtToken(user);
                var resetLink = Url.Action("ResetPassword", "Account", new { token }, "https");

                var emailContent = new Core.Dto.MailDtos.MailContent
                {
                    To = user.Email,
                    Subject = "Password Reset Request",
                    Body = $"<p>Please click the link below to reset your password.</p><a href='{resetLink}'>Reset Password</a>"
                };
                await _sendMailService.SendMail(emailContent);

                return Ok("Email khôi phục mật khẩu đã được gửi.");
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
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(request.Token) as JwtSecurityToken;

                if (jsonToken == null || !jsonToken.Claims.Any())
                {
                    return BadRequest("Invalid or expired token.");
                }
                if (await _tokenRepository.IsTokenUsedAsync(request.Token))
                {
                    return BadRequest("Token has already been used.");
                }
                var emailClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                var purposeClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "Purpose")?.Value;

                if (emailClaim == null || purposeClaim != "purpose")
                {
                    return BadRequest("Invalid token.");
                }

                var user = _accountRepository.GetByEmail(emailClaim);

                if (user == null)
                {
                    return BadRequest("User not found.");
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                _accountRepository.Update(user);
                await _accountRepository.Save();
                await _tokenRepository.MarkTokenAsUsedAsync(request.Token);

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

                var user = _accountRepository.GetByEmail(request.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
                {
                    return Unauthorized(new { Error = "Invalid email or password. Please try again" });
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
