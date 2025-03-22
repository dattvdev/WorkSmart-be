using Microsoft.AspNetCore.Mvc;
using WorkSmart.Core.Dto.CVDtos;
using WorkSmart.Application.Services;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;
using System.Text.RegularExpressions;
using System.Text;
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

        [HttpPost("upload-cv")]
        public async Task<IActionResult> UploadCV([FromBody] CvUploadDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.FilePath))
                {
                    return BadRequest(new { message = "Vui lòng cung cấp đường dẫn file." });
                }

                // Kiểm tra file có phải PDF không
                if (!request.FilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Chỉ hỗ trợ tệp PDF." });
                }

                //string cvContent = _cvService.ExtractCvContent(request.FilePath);
                var cvDto = await _cvService.UploadCVAsync(request.FilePath, request.UserId, request.FileName);

                return Ok(new
                {
                    message = "Trích xuất thành công",
                    cvDto,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xử lý file: " + ex.Message });
            }
        }

        [HttpPost("upload-read-cv")]
        public IActionResult UploadReadCV([FromBody] CvUploadDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.FilePath))
                {
                    return BadRequest(new { message = "Vui lòng cung cấp đường dẫn file." });
                }
                // Kiểm tra file có phải PDF không
                if (!request.FilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Chỉ hỗ trợ tệp PDF." });
                }

                // Gọi trực tiếp hàm ExtractCvContent để lấy nội dung
                string cvContent = _cvService.ExtractCvContent(request.FilePath);

                return Ok(new
                {
                    message = "Trích xuất thành công",
                    content = cvContent
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý file CV");
                return StatusCode(500, new { message = "Lỗi khi xử lý file: " + ex.Message });
            }
        }
    }
}

