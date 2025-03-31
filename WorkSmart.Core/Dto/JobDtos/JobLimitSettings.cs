using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.JobDtos
{
    public class JobLimitSettings
    {
        public int MaxJobsPerDay { get; set; } = 1; // Add the missing semicolon
        public string UpdatedAt { get; set; } = DateTime.Now.ToString(); // Default value
    }
}
