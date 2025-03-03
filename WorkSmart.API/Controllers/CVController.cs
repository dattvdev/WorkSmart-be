using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WorkSmart.Core.Dto.CVDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.API.Controllers
{
    [Route("cv")]
    [ApiController]
    public class CVController : ControllerBase
    {
        private readonly ICVRepository _cvRepository;
        private readonly IMapper _mapper;

        public CVController(ICVRepository cvRepository,IMapper mapper)
        {
            _cvRepository = cvRepository;
            _mapper = mapper;
        }


        [HttpPost]
        public async Task<IActionResult> CreateCV([FromBody] CreateCVDto createCvDto)
        {
            if (createCvDto == null)
                return BadRequest("Dữ liệu không hợp lệ");

            var cv = _mapper.Map<CV>(createCvDto);
            cv.UserID = 14; // Lấy từ user đăng nhập
            var newCV = await _cvRepository.CreateCVAsync(cv);

            return Ok(newCV);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCVById(int id)
        {
            var cv = await _cvRepository.GetById(id);
            if (cv == null)
                return NotFound();

            return Ok(_mapper.Map<CreateCVDto>(cv));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCVsByUserId(int userId)
        {
            var cvs = await _cvRepository.GetAllByUserIdAsync(userId);
            return Ok(_mapper.Map<IEnumerable<CreateCVDto>>(cvs));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCV(int id)
        {
            _cvRepository.Delete(id);
            return NoContent();
        }
    }
}
