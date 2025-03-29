using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.ReportDtos
{
    public class UpdateReportStatusDto
    {
        public string Status { get; set; } // "Completed" or "Rejected"
        public string? Reason { get; set; } // Optional reason for rejection
    }
}
