using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Helpers;

namespace WorkSmart.Core.Dto.CandidateDtos
{
    public class GetListSearchCandidateDto
    {
        public int UserID { get; set; }
        public string? UserName { get; set; }
        public string? Avatar { get; set; }
        public string Email { get; set; } 
        public string Role { get; set; }
        public string? ConfirmationCode { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsBanned { get; set; } = false;
        public bool? IsPrivated { get; set; }
        public double Amount { get; set; } = 0;
        public string? BankName { get; set; }
        public string? BankNumber { get; set; }
        public string FullName { get; set; } 
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public double? Exp { get; set; }
        public string? Skills { get; set; }
        public DateTime CreatedAt { get; set; } = TimeHelper.GetVietnamTime();

        public ICollection<Tag>? Tags { get; set; }
    }
}
