using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.JobDtos
{
    public class ExpiredJobDto
    {
        public int JobID { get; set; }
        public string Title { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime HiddenAt { get; set; }
    }
}
