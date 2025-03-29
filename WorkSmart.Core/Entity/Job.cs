using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using WorkSmart.Core.Enums;

namespace WorkSmart.Core.Entity
{
    
    public class Job
    {
        public int JobID { get; set; }
        public int UserID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Level { get; set; }
        public string? Education { get; set; }
        public int? NumberOfRecruitment { get; set; }
        public string? WorkType { get; set; }
        public string? Location { get; set; }
        public string? JobPosition { get; set; }
        //public double? Salary { get; set; }
        public string? Salary { get; set; }
        public int? Exp { get; set; }
        public bool Priority { get; set; } = false;
        public DateTime? Deadline { get; set; }
        public JobStatus? Status { get; set; }
        //public bool? IsHidden { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string? ReasonRejectedJob { get; set; }
        public string? CategoryID { get; set; }
        public User User { get; set; }
        public ICollection<Application>? Applications { get; set; }
        public ICollection<FavoriteJob>? FavoriteJobs { get; set; }
        public ICollection<Tag>? Tags { get; set; }
        public ICollection<ReportPost>? ReportPosts { get; set; }
    } 
}
