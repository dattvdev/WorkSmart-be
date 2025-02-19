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
        [HttpGet]
        public async Task<IEnumerable<GetListSearchCandidateDto>> GetListSearchCandidate(
                       int pageIndex, int pageSize,
                                  double? exp = null, List<int>? tagIds = null,
                                             string? address = null)
        {
            return await _candidateService.GetListSearchCandidate(pageIndex, pageSize, exp, tagIds, address);
        }
    }
}
