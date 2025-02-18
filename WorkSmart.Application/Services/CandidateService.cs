using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class CandidateService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IMapper _mapper;
        public CandidateService(ICandidateRepository candidateRepository, IMapper mapper)
        {
            _candidateRepository = candidateRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<GetListSearchCandidateDto>> GetListSearchCandidate(
            int pageIndex, int pageSize,
            double? exp = null, List<int>? tagIds = null,
            string? address = null)
        {
            var candidates = await _candidateRepository.GetListSearch(pageIndex, pageSize, exp, tagIds, address);
            return _mapper.Map<IEnumerable<GetListSearchCandidateDto>>(candidates);
        }
    }
}
