using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.JobDtos
{
    public class CandidateFreePlan
    {
        public int MaxCVsPerDay { get; set; } = 1;
        public string UpdatedAt { get; set; } = DateTime.Now.ToString();
    }
}
