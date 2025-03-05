using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.EmployerDtos;
using WorkSmart.Core.Interface;

namespace WorkSmart.API.Controllers
{
    [Route("employers")]
    [ApiController]
    public class EmployerController : ControllerBase
    {
        private readonly EmployerService _employerService;

        public EmployerController(EmployerService employerService)
        {
            _employerService = employerService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetCompanyProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null)
                {
                    return Unauthorized(new { Error = "UserId không tìm thấy trong token" });
                }

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

                var result = await _employerService.VerifyTax(userId, taxVerificationDto);
                if (result)
                    return Ok("Tax verification submitted successfully.");

                return BadRequest("Failed to submit tax verification.");
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
            var userId = int.Parse(User.FindFirst("UserId")?.Value);

            if (string.IsNullOrEmpty(request.BusinessLicenseImageUrl))
            {
                return BadRequest("Business license image URL is required.");
            }

            var result = await _employerService.UploadBusinessLicense(userId, request.BusinessLicenseImageUrl);

            if (!result)
            {
                return NotFound("User not found");
            }

            return Ok(new { message = "Business license submitted for verification." });
        }
    }
}
