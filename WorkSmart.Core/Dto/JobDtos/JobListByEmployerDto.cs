using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Dto.JobDtos
{
    public class JobListByEmployerDto
    {

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; }
        public string? Title { get; set; }
        public string? JobPosition { get; set; }
        public List<string>? WorkTypes { get; set; }
        public string? Location { get; set; }
        public double? MinSalary { get; set; }
        public double? MaxSalary { get; set; }
        public string? Category { get; set; }
        public List<int>? Tags { get; set; }
        public bool MostRecent { get; set; } = true;
        public int? UserID { get; set; }
    }
}
