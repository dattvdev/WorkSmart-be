using Microsoft.AspNetCore.Mvc;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.CVDtos;

namespace WorkSmart.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CVTemplatesController : ControllerBase
    {
        private readonly CVTemplateService _service;

        public CVTemplatesController(CVTemplateService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CVTemplateDto>>> GetAll()
        {
            var templates = await _service.GetAllTemplatesAsync();
            return Ok(templates);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CVTemplateDto>> GetById(int id)
        {
            var template = await _service.GetTemplateByIdAsync(id);
            if (template == null)
                return NotFound();
            return Ok(template);
        }

        [HttpPost]
        public async Task<ActionResult<CVTemplateDto>> Create(CVTemplateDto dto)
        {
            var created = await _service.CreateTemplateAsync(dto);
            return CreatedAtAction(nameof(GetById),new { id = created.CVTemplateId },created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id,CVTemplateDto dto)
        {
            if (id != dto.CVTemplateId)
                return BadRequest();

            var updated = await _service.UpdateTemplateAsync(dto);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteTemplateAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CVTemplateDto>>> Search([FromQuery] string name)
        {
            var results = await _service.SearchTemplatesByNameAsync(name);
            return Ok(results);
        }
    }

}
