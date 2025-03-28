﻿using System.ComponentModel.DataAnnotations;

namespace WorkSmart.Core.Entity
{
    public class CV_Certification
    {
        [Key]
        public int CertificationID { get; set; }
        public int CVID { get; set; }
        public string? CertificateName { get; set; }
        public string? Description { get; set; }
        public DateTime? CreateAt { get; set; }

        public CV CV { get; set; }
    }
}
