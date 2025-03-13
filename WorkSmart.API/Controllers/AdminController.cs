﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSmart.Application.Mapper;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.AdminDtos;
using WorkSmart.Core.Interface;

namespace WorkSmart.API.Controllers
{
    [Route("admins")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly AdminService _adminService;
        private readonly IMapper _mapper;

        public AdminController(IAccountRepository accountRepository, AdminService adminService, IMapper mapper)
        {
            _accountRepository = accountRepository;
            _adminService = adminService;
            _mapper = mapper;
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
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return BadRequest("Rejection reason is required.");
                }
                user.TaxVerificationStatus = "Rejected";
                user.TaxVerificationReason = request.Reason;
            }

            //Gửi mail cho employer sau khi đã approve/reject tax
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
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return BadRequest("Rejection reason is required.");
                }
                user.LicenseVerificationStatus = "Rejected";
                user.LicenseVerificationReason = request.Reason;
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
    }
}
