using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Helpers;
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

        public async Task<GetCandidateProfileDto?> GetCandidateProfile(int userId)
        {
            var user = await _candidateRepository.GetById(userId);

            if (user == null || user.Role != "Candidate")
                return null;

            return _mapper.Map<GetCandidateProfileDto>(user);
        }

        public async Task<bool> EditCandidateProfile(int userId, EditCandidateRequest request)
        {
            var user = await _candidateRepository.GetById(userId);

            if (user == null || user.Role != "Candidate")
                return false;

            // Cho phép null nếu người dùng muốn xóa
            if (request.FullName != null) user.FullName = request.FullName;
            if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;
            if (request.Gender != null) user.Gender = request.Gender;
            if (request.Address != null) user.Address = request.Address;
            if (request.IsPrivated != null) user.IsPrivated = (bool)request.IsPrivated;
            if (request.Avatar != null) user.Avatar = request.Avatar;

            user.UpdatedAt = TimeHelper.GetVietnamTime();
            _candidateRepository.Update(user);
            await _candidateRepository.Save();

            return true;
        }
    }
}
