using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Interface;

namespace WorkSmart.API.Controllers
{
    [Route("candidates")]
    [ApiController]
    public class CandidateController : ControllerBase
    {
        private readonly CandidateService _candidateService;
        private readonly IAccountRepository _accountRepository;

        public CandidateController(IAccountRepository accountRepository, CandidateService candidateService)
        {
            _accountRepository = accountRepository;
            _candidateService = candidateService;
        }

        public async Task<IEnumerable<GetListSearchCandidateDto>> GetListSearchCandidate([FromQuery] CandidateSearchRequestDto request)
        {
            return await _candidateService.GetListSearchCandidate(request);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetCandidateProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value);
                var user = await _accountRepository.GetById(userId);

                if (user == null || user.Role != "Candidate")
                {
                    return NotFound(new { Error = "Candidate not found." });
                }

                //Lấy các thông tin tương ứng ở FE
                var candidateProfile = new
                {
                    user.UserID,
                    user.FullName,
                    user.Email,
                    user.Avatar,
                    user.DateOfBirth,
                    user.Address
                };

                return Ok(candidateProfile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while getting profile" });
            }
        }

        [HttpPut("edit-profile")]
        public async Task<IActionResult> UpdateCandidateProfile([FromBody] UpdateCandidateRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value);
                var user = await _accountRepository.GetById(userId);

                if (user == null || user.Role != "Candidate")
                {
                    return NotFound(new { Error = "Candidate not found." });
                }

                // Cho phép null nếu người dùng muốn xóa
                if (request.FullName != null) user.FullName = request.FullName;
                if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;
                if (request.Gender != null) user.Gender = request.Gender;
                if (request.Address != null) user.Address = request.Address;
                if (request.Avatar != null) user.Avatar = request.Avatar;
                if (request.DateOfBirth != null) user.DateOfBirth = request.DateOfBirth;

                user.UpdatedAt = DateTime.UtcNow;
                _accountRepository.Update(user);
                await _accountRepository.Save();

                return Ok(new { Message = "Candidate profile updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while edit candidate profile" });
            }
        }
    }
}