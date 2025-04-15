using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.CVDtos;

namespace WorkSmart.Core.Dto.AccountDtos
{
    public class AccountWithFeaturedCVDto
    {
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string? Avatar { get; set; }
        public string? IdentityNumber { get; set; }
        public bool IdentityConfirmed { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsBanned { get; set; }
        public bool IsPrivated { get; set; }
        public double Amount { get; set; }
        public string? BankName { get; set; }
        public string? BankNumber { get; set; }
        public string FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyDescription { get; set; }
        public string? WorkLocation { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int VerificationLevel { get; set; }
        public string? TaxId { get; set; }
        public string? Industry { get; set; }
        public string? CompanySize { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? TaxVerificationStatus { get; set; }
        public string? TaxVerificationReason { get; set; }
        public string? BusinessLicenseImage { get; set; }
        public string? LicenseVerificationStatus { get; set; }
        public string? LicenseVerificationReason { get; set; }

        // Include all CVs with their details
        public List<FeaturedCVDto> FeaturedCVs { get; set; }
    }
}
