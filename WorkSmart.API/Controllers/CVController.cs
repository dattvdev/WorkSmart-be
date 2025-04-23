using Microsoft.AspNetCore.Mvc;
using WorkSmart.Core.Dto.CVDtos;
using WorkSmart.Application.Services;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;
using System.Text.RegularExpressions;
using System.Text;
using WorkSmart.Core.Interface;
using System.Security.Claims;
namespace WorkSmart.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CVController : ControllerBase
    {
        private readonly CVService _cvService;
        private readonly ICvParserService _cvParserService;
        private readonly ILogger<CVController> _logger;

        public CVController(CVService cvService,ILogger<CVController> logger, ICvParserService cvParserService)
        {
            _cvService = cvService;
            _logger = logger;
            _cvParserService = cvParserService;
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
                    return BadRequest(new { message = "Please provide file path." });
                }

                // Kiểm tra file có phải PDF không
                if (!request.FilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Only PDF files are accepted." });
                }

                //string cvContent = _cvService.ExtractCvContent(request.FilePath);
                var cvDto = await _cvService.UploadCVAsync(request.FilePath, request.UserId, request.FileName);

                return Ok(new
                {
                    message = "Extracted successfully",
                    cvDto,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing file: " + ex.Message });
            }
        }

        [HttpPost("upload-read-cv")]
        public IActionResult UploadReadCV([FromBody] CvUploadDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.FilePath))
                {
                    return BadRequest(new { message = "Please provide file path." });
                }
                // Kiểm tra file có phải PDF không
                if (!request.FilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Only PDF files are accepted." });
                }

                // Gọi trực tiếp hàm ExtractCvContent để lấy nội dung
                string cvContent = _cvParserService.ExtractCvContent(request.FilePath);

                return Ok(new
                {
                    message = "Extracted successfully",
                    content = cvContent
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing file: " + ex.Message });
            }
        }

        [HttpPost("upload-parse-cv")]
        public async Task<IActionResult> UploadParseCV([FromBody] CvUploadDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.FilePath))
                {
                    return BadRequest(new { message = "Please provide file path." });
                }

                if (!request.FilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Only PDF files are accepted." });
                }

                // Trích xuất nội dung CV
                string cvContent = _cvParserService.ExtractCvContent(request.FilePath);

                // Phân tích CV bằng AI và lưu vào database
                var parsedCvData = await _cvParserService.ParseCvAsync(cvContent, request.UserId, request.FilePath, request.FileName);

                return Ok(new
                {
                    message = "Extracted successfully",
                    parsedCvData
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "User authentication required" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing file: " + ex.Message });
            }
        }

        [HttpPut("hidecv/{cvId}")]
        public IActionResult HideCV(int cvId)
        {
            try
            {
                if (cvId == 0)
                {
                    return BadRequest(new { message = "Vui lòng cung cấp ID CV." });
                }
                _cvService.HideCV(cvId);
                return Ok(new
                {
                    message = "Ẩn thành công",
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi ẩn CV");
                return StatusCode(500, new { message = "Lỗi khi ẩn CV: " + ex.Message });
            }
        }

        [HttpGet("getRemainingCVCreationLimit/{userID}")]
        public async Task<ActionResult<CVCreationLimitDto>> GetRemainingCVCreationLimit(int userID)
        {
            var result = await _cvService.GetRemainingCVCreationLimit(userID);
            return Ok(result);
        }
    }
}

