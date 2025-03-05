using AutoMapper;
using WorkSmart.Core.Dto.PackageDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class PackageService
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IMapper _mapper;

        public PackageService(IPackageRepository packageRepository, IMapper mapper)
        {
            _packageRepository = packageRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<GetPackageDto>> GetAll()
        {
            var packages = await _packageRepository.GetAll();
            return _mapper.Map<IEnumerable<GetPackageDto>>(packages);
        }
        public async Task Add(CreateUpdatePackageDto createPackageDto)
        {
            var package = _mapper.Map<Package>(createPackageDto);
            await _packageRepository.Add(package);
        }
        public async Task<GetPackageDto> GetById(int id)
        {
            var package = await _packageRepository.GetById(id);
            return _mapper.Map<GetPackageDto>(package);
        }
        public void Delete(int id)
        {
            _packageRepository.Delete(id);
        }
    }
}
