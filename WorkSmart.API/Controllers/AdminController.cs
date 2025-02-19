using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSmart.Core.Interface;

namespace WorkSmart.API.Controllers
{
    [Route("admins")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;

        public AdminController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        [HttpGet("test-auth")]
        public IActionResult TestAuth()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Admin" || userRole == "Employer" || userRole == "Candidate")
            {
                return Ok($"Authorized with role: {userRole}");
            }
            return Unauthorized("Access denied");
        }

        [HttpPost("ban/{id}")]
        public async Task<IActionResult> BanUser(int id)
        {
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (currentUserRole != "Admin")
            {
                return Forbid();
            }

            var user = await _accountRepository.GetById(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            if (user.IsBanned)
            {
                return BadRequest(new { Message = "This user is already banned" });
            }

            user.IsBanned = true;
            _accountRepository.Update(user);

            return Ok(new { Message = $"User {user.Email} has been banned successfully" });
        }

        [HttpPost("unban/{id}")]
        public async Task<IActionResult> UnbanUser(int id)
        {
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (currentUserRole != "Admin")
            {
                return Forbid();
            }

            var user = await _accountRepository.GetById(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            if (!user.IsBanned)
            {
                return BadRequest(new { Message = "This user is not banned yet" });
            }

            user.IsBanned = false;
            _accountRepository.Update(user);

            return Ok(new { Message = $"User {user.Email} has been unbanned successfully" });
        }
    }
}
