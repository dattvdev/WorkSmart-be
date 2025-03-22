using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Enums;

namespace WorkSmart.Core.Dto.JobDtos
{
    public class ExpiredJobsResultDto
    {
        public int HiddenCount { get; set; }
        public List<ExpiredJobDto> HiddenJobs { get; set; }
    }
}
