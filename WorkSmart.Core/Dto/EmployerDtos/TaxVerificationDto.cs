using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.EmployerDtos
{
    public class TaxVerificationDto
    {
        public string? TaxId { get; set; }
        public string? Industry { get; set; }
        public string? CompanySize { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyDescription { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}
