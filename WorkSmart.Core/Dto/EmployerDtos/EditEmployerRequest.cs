using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.EmployerDtos
{
    public class EditEmployerRequest
    {
        [RegularExpression(@"^\d{10,}$", ErrorMessage = "Phone number must contain only numbers and be at least 10 digits.")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        [MinLength(3, ErrorMessage = "Company Name must be at least 3 characters.")]
        [RegularExpression(@"^[A-Za-zÀ-Ỹà-ỹ\s]+$", ErrorMessage = "Company Name must contain only letters.")]
        public string? CompanyName { get; set; }

        [MinLength(10, ErrorMessage = "Company Description must be at least 10 characters.")]
        public string? CompanyDescription { get; set; }
        
        public DateTime? CreatedAt { get; set; }
        
        public string? Avatar { get; set; }
        
        public bool? IsPrivated { get; set; }
    }
}
