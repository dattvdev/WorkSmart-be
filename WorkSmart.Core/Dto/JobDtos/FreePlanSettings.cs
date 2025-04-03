using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.JobDtos
{
    public class FreePlanSettings
    {
        public EmployerFreePlan employerFreePlan { get; set; }
        public CandidateFreePlan candidateFreePlan { get; set; }
    }
}
