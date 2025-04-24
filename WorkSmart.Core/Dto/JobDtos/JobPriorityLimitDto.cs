using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.JobDtos
{
    public class JobPriorityLimitDto
    {
        public int RemainingLimit { get; set; }
        public int TotalLimit { get; set; }
        public int UsedToday { get; set; }
        public string Message { get; set; }
        public string PackageName { get; set; }
    }
}
