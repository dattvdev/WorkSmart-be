﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Entity
{
    public class CV
    {
        public int CVID { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string? JobPosition { get; set; }
        public string? Summary { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Link { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User User { get; set; }
        public ICollection<Application>? Applications { get; set; }
        public ICollection<CV_Education>? Educations { get; set; }
        public ICollection<CV_Experience>? Experiences { get; set; }
        public ICollection<CV_Certification>? Certifications { get; set; }
        public ICollection<CV_Skill>? Skills { get; set; }
    }
}
