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
                var userId = int.Parse(User.FindFirst("UserId")?.Value);
                var profile = await _employerService.GetEmployerProfile(userId);

                if (profile == null)
                {
                    return NotFound(new {Error = "Employer not found"});
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while getting profile." });
            }
        }

        [HttpPut("edit-profile")]
        public async Task<IActionResult> UpdateEmployerProfile([FromBody] UpdateEmployerRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value);
                var isUpdated = await _employerService.UpdateEmployerProfile(userId, request);

                if (!isUpdated)
                {
                    return NotFound(new { Error = "Employer not found" });
                }

                return Ok(new { Message = "Candidate profile updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while updating profile." });
            }
        }
    }
}
