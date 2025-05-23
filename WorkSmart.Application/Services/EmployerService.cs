﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Dto.EmployerDtos;
using WorkSmart.Core.Helpers;
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
            
            return _mapper.Map<GetEmployerProfileDto>(user);
        }

        public async Task<bool> EditEmployerProfile(int userId, EditEmployerRequest request)
        {
            var user = await _accountRepository.GetById(userId);
           
            // Cho phép null nếu người dùng muốn xóa
            if (request.CompanyName != null) user.CompanyName = request.CompanyName;
            if (request.CompanyDescription != null) user.CompanyDescription = request.CompanyDescription;
            if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;
            if (request.CreatedAt != null) user.CreatedAt = (DateTime)request.CreatedAt;
            if (request.Address != null) user.Address = request.Address;
            if (request.Avatar != null) user.Avatar = request.Avatar;

            user.UpdatedAt = TimeHelper.GetVietnamTime();
            _accountRepository.Update(user);
            await _accountRepository.Save();

            return true;
        }

        public async Task<bool> VerifyTax(int userId, TaxVerificationDto request)
        {
            var user = await _accountRepository.GetById(userId);
            
            if (user.VerificationLevel >= 2)
            {
                throw new InvalidOperationException("Tax verification already completed.");
            }

            if (user.TaxVerificationStatus == "Pending")
            {
                throw new InvalidOperationException("Authentication request has been sent.");
            }
            
            if (user.TaxVerificationStatus == "Approved")
            {
                throw new InvalidOperationException("Tax code already verified.");
            }

            user.TaxId = request.TaxId;
            user.Industry = request.Industry;
            user.CompanySize = request.CompanySize;
            user.CompanyName = request.CompanyName;
            user.CompanyDescription = request.CompanyDescription;
            user.PhoneNumber = request.PhoneNumber;
            user.Address = request.Address;
            user.CompanyWebsite = request.CompanyWebsite;
            user.TaxVerificationStatus = "Pending";
            user.TaxVerificationReason = null;
            user.UpdatedAt = TimeHelper.GetVietnamTime();

            _accountRepository.Update(user);
            await _accountRepository.Save();

            return true;
        }

        public async Task<bool> UploadBusinessLicense(int userId, string imageUrl)
        {
            var user = await _accountRepository.GetById(userId);
           
            if (user.VerificationLevel == 1)
            {
                throw new InvalidOperationException("Company must verify tax first");
            }

            if (user.VerificationLevel >= 3)
            {
                throw new InvalidOperationException("Business License verification already completed.");
            }

            if (user.LicenseVerificationStatus == "Pending")
            {
                throw new InvalidOperationException("Authentication request has been sent.");
            }

            if (user.LicenseVerificationStatus == "Approved")
            {
                throw new InvalidOperationException("Business license already verified.");
            }

            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new ArgumentException("Business license image URL is required.");
            }

            user.BusinessLicenseImage = imageUrl;
            user.LicenseVerificationStatus = "Pending";
            user.LicenseVerificationReason = null;
            user.UpdatedAt = TimeHelper.GetVietnamTime();

            _accountRepository.Update(user);
            await _accountRepository.Save();

            return true;
        }
    }
}
