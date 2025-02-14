using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Enums;

namespace WorkSmart.Core.Dto.JobDtos
{
    public class UpdateJobDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public double? Salary { get; set; }
        public JobStatus? Status { get; set; }
    }
}
