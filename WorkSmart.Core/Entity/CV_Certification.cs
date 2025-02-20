using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Entity
{
    public class CV_Certification
    {
        [Key]
        public int CertificationID { get; set; }
        public int CVID { get; set; }
        public string CertificateName { get; set; }
        public string? Description { get; set; }
        public DateTime CreateAt { get; set; }

        public CV CV { get; set; }
    }
}
