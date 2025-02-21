using Microsoft.AspNetCore.Mvc;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.CandidateDtos;

namespace WorkSmart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController : Controller
    {
        private readonly CandidateService _candidateService;
        public CandidateController(CandidateService candidateService)
        {
            _candidateService = candidateService;
        }
        // GET: api/GetListSearchCandidate
        [HttpGet("GetListSearch")]
        public async Task<IActionResult> GetListSearchCandidate
            ([FromQuery] CandidateSearchRequestDto request)
        {
            var (candidates, total) = await _candidateService.GetListSearchCandidate(request);
            var totalPage = (int)Math.Ceiling((double)total / request.PageSize);
            return Ok(new { totalPage, candidates });
        }
    }
}
