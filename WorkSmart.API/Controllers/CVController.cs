using Microsoft.AspNetCore.Mvc;
using WorkSmart.Core.Dto.CVDtos;
using WorkSmart.Application.Services;
namespace WorkSmart.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CVController : ControllerBase
    {
        private readonly CVService _cvService;
        private readonly ILogger<CVController> _logger;

        public CVController(CVService cvService,ILogger<CVController> logger)
        {
            _cvService = cvService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<ActionResult<CVDto>> CreateCV([FromBody] CVDto cvDto)
        {
            if (cvDto == null)
            {
                return BadRequest("CV data cannot be null.");
            }
            cvDto.CVID = 0;
            var createdCv = await _cvService.CreateCVAsync(cvDto);
            return CreatedAtAction(nameof(GetCV),new { id = createdCv.CVID },createdCv);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CVDto>> GetCV(int id)
        {
            var cv = await _cvService.GetCVByIdAsync(id);
            if (cv == null)
            {
                return NotFound();
            }

            return Ok(cv);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<CVDto>>> GetAllCVsByUserId(int userId)
        {
            var cvs = await _cvService.GetAllCVsAsync(userId);
            return Ok(cvs);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CVDto>> UpdateCV(int id,[FromBody] CVDto cvDto)
        {
            if (id != cvDto.CVID)
            {
                return BadRequest("CV ID mismatch.");
            }

            try
            {
                var updatedCv = await _cvService.UpdateCVAsync(1,cvDto); // Giả định 1 là ID user
                return Ok(updatedCv); // Trả về CV đã cập nhật
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // Trả về 403 Forbidden nếu không có quyền
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message); // Trả về 404 nếu không tìm thấy
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCV(int id)
        {
            await _cvService.DeleteCVAsync(id);
            return NoContent();
        }

        [HttpPut("/setfeature")]
        public void SetFeature(int cvId, int userId)
        {
            _cvService.SetFeature(cvId, userId);
        }
    }
}
