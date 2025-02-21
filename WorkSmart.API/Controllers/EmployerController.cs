using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkSmart.Core.Dto.EmployerDtos;
using WorkSmart.Core.Interface;

namespace WorkSmart.API.Controllers
{
    [Route("employers")]
    [ApiController]
    public class EmployerController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;

        public EmployerController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetCompanyProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value);
                var user = await _accountRepository.GetById(userId);

                if (user == null || user.Role != "Employer")
                {
                    return NotFound(new {Error = "Employer not found"});
                }

                var CompanyProfile = new
                {
                    user.FullName,
                    user.PhoneNumber,
                    user.Address,
                    user.Amount,
                    user.BankName,
                    user.BankNumber,
                    user.CompanyName,
                    user.CompanyDescription,
                    user.WorkLocation
                };

                return Ok(CompanyProfile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("edit-profile")]
        public async Task<IActionResult> UpdateEmployerProfile([FromBody] UpdateEmployerRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value);
                var user = await _accountRepository.GetById(userId);

                if (user == null || user.Role != "Employer")
                {
                    return NotFound(new { Error = "Employer not found" });
                }

                // Cho phép null nếu người dùng muốn xóa
                if (request.FullName != null) user.FullName = request.FullName;
                if (request.PhoneNumber !=  null) user.PhoneNumber = request.PhoneNumber;
                if (request.Address != null) user.Address = request.Address;
                if (request.Amount != null) user.Amount = request.Amount;
                if (request.BankName != null) user.BankName = request.BankName;
                if (request.BankNumber != null) user.BankNumber = request.BankNumber;
                if (request.CompanyName != null) user.CompanyName = request.CompanyName;
                if (request.CompanyDescription != null) user.CompanyDescription = request.CompanyDescription;
                if (request.WorkLocation != null) user.WorkLocation = request.WorkLocation;

                user.UpdatedAt = DateTime.UtcNow;
                _accountRepository.Update(user);
                await _accountRepository.Save();

                return Ok(new { Message = "Candidate profile updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
