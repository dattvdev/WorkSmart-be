using AutoMapper;
using WorkSmart.Core.Dto.CVDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class CVService 
    {
        private readonly ICVRepository _cvRepository;
        private readonly IMapper _mapper;

        public CVService(ICVRepository cvRepository,IMapper mapper)
        {
            _cvRepository = cvRepository;
            _mapper = mapper;
        }

        public async Task<CVDto> CreateCVAsync(CVDto cvDto)
        {
            var cv = _mapper.Map<CV>(cvDto);
            await _cvRepository.Add(cv);
            return _mapper.Map<CVDto>(cv);
        }

        public async Task<CVDto> GetCVByIdAsync(int id)
        {
            var cv = await _cvRepository.GetCVWithDetails(id);
            return _mapper.Map<CVDto>(cv);
        }

        public async Task<IEnumerable<CVDto>> GetAllCVsAsync(int userId)
        {
            var cvs = await _cvRepository.GetAllCVsByUserId(userId);
            return _mapper.Map<IEnumerable<CVDto>>(cvs);
        }

        public async Task<CVDto> UpdateCVAsync(int userId, CVDto cvDto)
        {
            var existingCv = await _cvRepository.GetCVWithDetails(cvDto.CVID);

            if (existingCv == null)
            {
                throw new KeyNotFoundException($"CV with ID {cvDto.CVID} not found.");
            }
            

            if (existingCv.UserID != cvDto.UserID)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this CV.");
            }

            
            _mapper.Map(cvDto,existingCv); // Ánh xạ tất cả thuộc tính từ DTO vào entity
            existingCv.UpdatedAt = DateTime.Now;
            _cvRepository.Update(existingCv);
            return _mapper.Map<CVDto>(existingCv);  // Trả về CV được ánh xạ trở lại DTO
        }

        public async Task DeleteCVAsync(int id)
        {
            await _cvRepository.Delete(id); 
        }

        public void SetFeature(int cvId, int userId)
        {
            _cvRepository.SetFeature(cvId, userId);
        }

    }
}