using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Dto.EmployerDtos;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class EmployerService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        public EmployerService(IAccountRepository accountRepository, IMapper mapper)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;
        }

        public async Task<GetEmployerProfileDto> GetEmployerProfile(int userId)
        {
            var user = await _accountRepository.GetById(userId);

            if (user == null || user.Role != "Employer")
                return null;

            return _mapper.Map<GetEmployerProfileDto>(user);
        }

        public async Task<bool> UpdateEmployerProfile(int userId, UpdateEmployerRequest request)
        {
            var user = await _accountRepository.GetById(userId);
            if (user == null || user.Role != "Employer")
            {
                return false;
            }

            // Cho phép null nếu người dùng muốn xóa
            if (request.FullName != null) user.FullName = request.FullName;
            if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;
            if (request.Address != null) user.Address = request.Address;
            if (request.Amount != null) user.Amount = request.Amount;
            if (request.BankName != null) user.BankName = request.BankName;
            if (request.BankNumber != null) user.BankNumber = request.BankNumber;
            if (request.CompanyName != null) user.CompanyName = request.CompanyName;
            if (request.CompanyDescription != null) user.CompanyDescription = request.CompanyDescription;
            if (request.WorkLocation != null) user.WorkLocation = request.WorkLocation;

            user.UpdatedAt = DateTime.UtcNow;
            _accountRepository.Update(user);
            await _accountRepository.Save();

            return true;
        }
    }
}
