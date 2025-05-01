using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Helpers;

namespace WorkSmart.Core.Entity
{
    public class ReportPost
    {
        public int ReportPostID { get; set; }
        public int SenderID { get; set; }
        public int JobID { get; set; }
        public string? Content { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Completed, Rejected
        public DateTime CreatedAt { get; set; } = TimeHelper.GetVietnamTime();
        public User Sender { get; set; }
        public Job Job { get; set; }
    }
}
