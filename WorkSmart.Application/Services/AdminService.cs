using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.AdminDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;
using WorkSmart.Repository.Repository;

namespace WorkSmart.Application.Services
{
    public class AdminService
    {
        private readonly IAccountRepository _accountRepository;

        public AdminService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<List<GetListVerificationDto>> GetPendingVerifications()
        {
            var users = await _accountRepository.GetAll();

            var pendingVerifications = users
                .Where(u => u.Role == "Employer" &&
                            (u.TaxVerificationStatus == "Pending" || u.LicenseVerificationStatus == "Pending"))
                .Select(u => new GetListVerificationDto
                {
                    UserID = u.UserID,
                    CompanyName = u.CompanyName,
                    CompanyDescription = u.CompanyDescription,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Industry = u.Industry,
                    CompanySize = u.CompanySize,

                    // Xác thực thuế
                    TaxId = u.TaxId,
                    TaxVerificationStatus = u.TaxVerificationStatus,
                    TaxVerificationReason = u.TaxVerificationReason,

                    // Xác thực GPKD
                    BusinessLicenseImage = u.BusinessLicenseImage,
                    LicenseVerificationStatus = u.LicenseVerificationStatus,
                    LicenseVerificationReason = u.LicenseVerificationReason,

                    VerificationLevel = u.VerificationLevel
                })
                .ToList();

            return pendingVerifications;
        }
    }
}
