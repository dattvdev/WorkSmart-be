using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;

namespace WorkSmart.Core.Dto.JobDtos
{
    public class CreateJobDto
    {
        public int UserID { get; set; }
        public List<int>? JobTagID { get; set; }
        public List<Tag>? Tags { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Level { get; set; }
        public string? Education { get; set; }
        public int? NumberOfRecruitment { get; set; }
        public string? WorkType { get; set; }
        public string? Location { get; set; }
        public double? Salary { get; set; }
        public int? Exp { get; set; }
        public bool Priority { get; set; } = false;
        public DateTime? Deadline { get; set; }
        public JobStatus? Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
