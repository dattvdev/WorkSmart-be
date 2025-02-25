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

        public CandidateController(CandidateService candidateService)
        {
            _candidateService = candidateService;
        }

        [HttpGet("GetListSearch")]
        public async Task<IActionResult> GetListSearchCandidate
            ([FromQuery] CandidateSearchRequestDto request)
        {
            var (candidates, total) = await _candidateService.GetListSearchCandidate(request);
            var totalPage = (int)Math.Ceiling((double)total / request.PageSize);
            return Ok(new { totalPage, candidates });
        }

        //[HttpGet("profile")]
        //public async Task<IActionResult> GetCandidateProfile()
        //{
        //    try
        //    {
        //        var userId = int.Parse(User.FindFirst("UserId")?.Value);

        //        var candidateProfile = await _candidateService.GetCandidateProfile(userId);

        //        if (candidateProfile == null)
        //            return NotFound(new { Error = "Candidate not found." });

        //        return Ok(candidateProfile);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Error = "An error occurred while getting profile" });
        //    }
        //}

        [HttpPut("edit-profile")]
        public async Task<IActionResult> EditCandidateProfile([FromBody] EditCandidateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Error = "Invalid input data.", Details = ModelState });
                }

                var userId = int.Parse(User.FindFirst("UserId")?.Value);

                var isUpdated = await _candidateService.UpdateCandidateProfile(userId, request);

                if (!isUpdated)
                    return NotFound(new { Error = "Candidate not found." });

                return Ok(new { Message = "Candidate profile updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while editing candidate profile" });
            }
        }
    }
}