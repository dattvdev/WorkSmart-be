using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.EmployerDtos;
using WorkSmart.Core.Dto.UserDtos;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserMessageInfoDto> GetUserMessageInfo(int userID)
        {
            var users = await _userRepository.GetById(userID);
            return _mapper.Map<UserMessageInfoDto>(users);
        }

        public async Task<CompanyDto> GetEmployerByCompanyName(string companyName)
        {
            var users = await _userRepository.GetEmployerByCompanyName(companyName);
            return _mapper.Map<CompanyDto>(users);
        }
    }
}
