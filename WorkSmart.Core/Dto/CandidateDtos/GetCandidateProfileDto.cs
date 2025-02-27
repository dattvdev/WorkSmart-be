using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.CandidateDtos
{
    public class GetCandidateProfileDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        //public DateTime? DateOfBirth { get; set; }
        public string IdentityNumber { get; set; }
        public bool IsPrivated { get; set; }
    }
}
