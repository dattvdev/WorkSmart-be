using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Entity
{
    public class CV_Education
    {
        [Key]
        public int EducationID { get; set; }
        public int CVID { get; set; }
        public string? Major { get; set; }
        public string? SchoolName { get; set; }
        public string? Degree { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public CV CV { get; set; }
    }
}
