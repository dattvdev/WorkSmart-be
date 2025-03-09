using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.AdminDtos
{
    public class GetListVerificationDto
    {
        public int? UserID { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyDescription { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Industry { get; set; }
        public string? CompanySize { get; set; }

        // Xác thực thuế
        public string? TaxId { get; set; }
        public string? TaxVerificationStatus { get; set; }
        public string? TaxVerificationReason { get; set; }

        // Xác thực GPKD
        public string? BusinessLicenseImage { get; set; }
        public string? LicenseVerificationStatus { get; set; }
        public string? LicenseVerificationReason { get; set; }

        public int VerificationLevel { get; set; }
    }
}
