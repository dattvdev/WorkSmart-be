using AutoMapper;
using WorkSmart.Core.Dto.CVDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class CVTemplateService
    {
        private readonly ICVTemplateRepository _repository;
        private readonly IMapper _mapper;

        public CVTemplateService(ICVTemplateRepository repository,IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CVTemplateDto>> GetAllTemplatesAsync()
        {
            var templates = await _repository.GetAll();
            return _mapper.Map<IEnumerable<CVTemplateDto>>(templates);
        }

        public async Task<CVTemplateDto> GetTemplateByIdAsync(int id)
        {
            var template = await _repository.GetById(id);
            return _mapper.Map<CVTemplateDto>(template);
        }

        public async Task<CVTemplateDto> CreateTemplateAsync(CVTemplateDto dto)
        {
            var entity = _mapper.Map<CV_Template>(dto);
            await _repository.Add(entity);
            return _mapper.Map<CVTemplateDto>(entity);
        }

        public async Task<bool> UpdateTemplateAsync(CVTemplateDto dto)
        {
            var entity = await _repository.GetById(dto.CVTemplateId);
            if (entity == null)
                return false;

            _mapper.Map(dto,entity);
            _repository.Update(entity);
            await _repository.Save();
            return true;
        }

        public async Task<bool> DeleteTemplateAsync(int id)
        {
            var entity = await _repository.GetById(id);
            if (entity == null)
                return false;

            _repository.Delete(id);
            await _repository.Save();
            return true;
        }

        public async Task<IEnumerable<CVTemplateDto>> SearchTemplatesByNameAsync(string name)
        {
            var templates = await _repository.GetTemplatesByNameAsync(name);
            return _mapper.Map<IEnumerable<CVTemplateDto>>(templates);
        }
    }
}
