using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.EmployerDtos
{
    public class EditEmployerRequest
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public double Amount { get; set; }
        public string? BankName { get; set; }
        public string? BankNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyDescription { get; set; }
        public string? WorkLocation { get; set; }
    }
}
