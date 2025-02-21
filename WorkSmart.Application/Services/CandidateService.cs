using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Entity;
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
        public async Task<(IEnumerable<GetListSearchCandidateDto> Users, int Total)> GetListSearchCandidate(CandidateSearchRequestDto request)
        {
            var (users, total) = await _candidateRepository.GetListSearch(request);

            var mappedUsers = _mapper.Map<IEnumerable<GetListSearchCandidateDto>>(users);

            return (mappedUsers, total);
        }
    }
}
