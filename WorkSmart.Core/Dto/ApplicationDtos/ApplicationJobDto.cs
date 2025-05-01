using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;
using WorkSmart.Core.Helpers;

namespace WorkSmart.Core.Dto.ApplicationDtos
{
    public class ApplicationJobDto
    {
        public int ApplicationID { get; set; }
        public int UserID { get; set; }
        public int JobID { get; set; }
        public int CVID { get; set; }
        public string? PhoneNumber { get; set; }
        public string? RejectionReason { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string Gender { get; set; }
        public string? Avatar { get; set; }
        public string? ApplicationStatus { get; set; }
        //job
        public string? CompanyAvatar { get; set; }
        public string? CompanyName { get; set; }
        public string? Title { get; set; }
        public string? CategoryID { get; set; }
        public string Description { get; set; }
        public string? Level { get; set; }
        public string? Education { get; set; }
        public int? NumberOfRecruitment { get; set; }
        public string? WorkType { get; set; }
        public string? Location { get; set; }
        public string? JobPosition { get; set; }
        public string? Salary { get; set; }
        public int? Exp { get; set; }
        public bool? Priority { get; set; } = false;
        public DateTime? Deadline { get; set; }
        public JobStatus? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = TimeHelper.GetVietnamTime();
        public int? MaxJobsPerDay { get; set; }

        //public ICollection<CV_Education> CV_Educations { get; set; }
        //public ICollection<CV_Experience> CV_Experiences { get; set; }
        //public ICollection<CV_Skill> CV_Skills { get; set; }
    }
}
