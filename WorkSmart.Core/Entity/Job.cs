using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace WorkSmart.Core.Entity
{
    public class Job
    {
        public int JobID { get; set; }
        public int UserID { get; set; }
        public int JobTagID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Location { get; set; }
        public double? Salary { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User User { get; set; }
        public ICollection<Application>? Applications { get; set; }
        public ICollection<FavoriteJob>? FavoriteJobs { get; set; }
        public JobTag JobTag { get; set; }
        public ICollection<ReportPost>? ReportPosts { get; set; }
    }
}
