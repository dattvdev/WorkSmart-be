using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Dto.EmployerDtos
{
    public class CompanyDto
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string? Gender { get; set; }
        public string FullName { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public string? Industry { get; set; } 
        public string? CompanySize { get; set; }
        public string CompanyName { get; set; }
        public string CompanyWebsite { get; set; }
        public string CompanyDescription { get; set; }
        public string? WorkLocation { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Avatar { get; set; }
        public int VerificationLevel { get; set; }
        public string BusinessLicenseImage { get; set; }
        public string TaxVerificationStatus { get; set; }
        public string LicenseVerificationStatus { get; set; }
        public ICollection<CompanyJobDto>? PostedJobs { get; set; }
    }
}
