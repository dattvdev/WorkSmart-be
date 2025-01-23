using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Entity
{
    public class CV_Experience
    {
        [Key]
        public int ExperienceID { get; set; }
        public int CVID { get; set; }
        public string JobPosition { get; set; }
        public string CompanyName { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public CV CV { get; set; }
    }
}
