using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.AccountDtos
{
    public class RegisterRequest
    {
        public string FullName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Role { get; set; } //candidate and employer

        public string? Avatar { get; set; }

        //employer
        public string? PhoneNumber { get; set; }

        public string? Gender { get; set; }

        public string? CompanyName { get; set; }

        public string? Address { get; set; }
    }
}
