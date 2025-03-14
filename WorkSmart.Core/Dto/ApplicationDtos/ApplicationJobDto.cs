using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Dto.ApplicationDtos
{
    public class ApplicationJobDto
    {
        public int ApplicationID { get; set; }
        public int UserID { get; set; }
        public int JobID { get; set; }
        public int CVID { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string? PhoneNumber { get; set; }
        public string? RejectionReason { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string Gender { get; set; }
    }
}
