using Microsoft.AspNetCore.Mvc;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.PackageDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController : Controller
    {
        private readonly PackageService _packageService;

        public PackageController(PackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var packages = await _packageService.GetAll();
                return Ok(packages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("getById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var package = await _packageService.GetById(id);
                return Ok(package);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] CreateUpdatePackageDto package)
        {
            try
            {
                await _packageService.Add(package);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<GetPackageDto>> Update(int id,GetPackageDto dto)
        {
            if (id != dto.PackageID)
                return BadRequest();

            var updatedPackage = await _packageService.UpdatePackageAsync(dto);
            if (updatedPackage == null)
                return NotFound();

            return Ok(updatedPackage);
        }

        [HttpDelete("delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _packageService.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
