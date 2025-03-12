using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkSmart.Application.Services;

namespace WorkSmart.API.Controllers
{
    [Route("uploads")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly CloudinaryService _cloudinaryService;

        public UploadController(CloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please select pictures valid");

            var imageUrl = await _cloudinaryService.UploadImage(file, "profile_pictures");
            return Ok(new {ImageUrl = imageUrl});
        }

        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please select a valid file.");

            var fileUrl = await _cloudinaryService.UploadFile(file, "documents");
            return Ok(new { FileUrl = fileUrl });
        }

        [HttpDelete("delete-image")]
        public async Task<IActionResult> DeleteImage([FromBody] string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return BadRequest("Image URL is required.");

            var result = await _cloudinaryService.DeleteImage(imageUrl);
            if (result)
                return Ok(new { Message = "Image deleted successfully." });

            return BadRequest("Failed to delete image.");
        }
    }
}
