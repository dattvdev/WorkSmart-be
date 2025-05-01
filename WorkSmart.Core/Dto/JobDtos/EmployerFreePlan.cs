using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Helpers;

namespace WorkSmart.Core.Dto.JobDtos
{
    public class EmployerFreePlan
    {
        public int MaxJobsPerDay { get; set; } = 1; // Add the missing semicolon
        public string UpdatedAt { get; set; } = TimeHelper.GetVietnamTime().ToString(); // Default value
        public int DefaultFeaturedJob { get; set; } = 1;
    }
}
