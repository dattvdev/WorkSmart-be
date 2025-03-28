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
        public string? ReportContent { get; set; }
        public string ReportStatus { get; set; }
        public DateTime CreatedAt { get; set; }

        public AccountDto Sender { get; set; }

        // Job Details
        public JobDto Job { get; set; }
    }
}
