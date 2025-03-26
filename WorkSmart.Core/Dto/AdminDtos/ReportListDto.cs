using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.AdminDtos
{
    public class ReportListDto
    {
        public int ReportPostID { get; set; }
        public int SenderID { get; set; }
        public string SenderName { get; set; }
        public string SenderAvatar { get; set; }
        public int JobID { get; set; }
        public string JobTitle { get; set; }
        public string ReportTitle { get; set; }
        public string? ReportContent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
