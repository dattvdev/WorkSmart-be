using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Dto.CandidateDtos
{
    public class GetListSearchCandidateDto
    {
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string Email { get; set; } //candidate and employer
        public string Role { get; set; } //candidate and employer
        public string? ConfirmationCode { get; set; }
        public bool IsEmailConfirmed { get; set; } //candidate and employer
        public bool IsBanned { get; set; } = false;
        public double Amount { get; set; } = 0;
        public string? BankName { get; set; }
        public string? BankNumber { get; set; }
        public string FullName { get; set; } //candidate and employer
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public double? Exp { get; set; }
        public string? Skills { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Tag>? Tags { get; set; }
    }
}
