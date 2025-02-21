using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.CandidateDtos
{
    public class UpdateCandidateRequest
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? Skills { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public double? Exp { get; set; }
        public string? Avatar { get; set; }
    }
}
