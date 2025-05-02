using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.CandidateDtos
{
    public class EditCandidateRequest
    {
        [MinLength(2, ErrorMessage = "Full name must be at least 2 characters")]
        public string? FullName { get; set; }

        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        public string? Gender { get; set; }

        public string? Address { get; set; }

        //public DateTime? DateOfBirth { get; set; }

        public bool? IsPrivated { get; set; }

        public string? Avatar { get; set; }
    }
}
