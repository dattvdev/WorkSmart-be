using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
                return StatusCode(500, new { Error = "An error occurred while getting profile." });
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
                return StatusCode(500, new { Error = "An error occurred while updating profile." });
            }
        }
    }
}
