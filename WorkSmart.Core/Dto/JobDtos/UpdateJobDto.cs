using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;

namespace WorkSmart.Core.Dto.JobDtos
{
    public class UpdateJobDto
    {
        public string Title { get; set; }
        public string? CategoryID { get; set; }
        public List<int>? Tags { get; set; }
        public string Description { get; set; }
        public string? Level { get; set; }
        public string? Education { get; set; }
        public int? NumberOfRecruitment { get; set; }
        public string? WorkType { get; set; }
        public string? Location { get; set; }
        //public double? Salary { get; set; }
        public string? Salary { get; set; }
        public int? Exp { get; set; }
        public bool Priority { get; set; }
        public DateTime? Deadline { get; set; }
        public JobStatus? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

    }
}
