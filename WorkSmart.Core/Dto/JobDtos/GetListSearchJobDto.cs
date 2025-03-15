using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;

namespace WorkSmart.Core.Dto.JobDtos
{
    public class GetListSearchJobDto
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
        public double? Salary { get; set; }
        public bool? IsHidden { get; set; }
        public int? Exp { get; set; }
        public bool Priority { get; set; } = false;
        public string? Avatar { get; set; }

        public DateTime? Deadline { get; set; }
        public JobStatus? Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<FavoriteJob>? FavoriteJobs { get; set; }
        public ICollection<Tag>? Tags { get; set; }
        public ICollection<ReportPost>? ReportPosts { get; set; }
    }
}
