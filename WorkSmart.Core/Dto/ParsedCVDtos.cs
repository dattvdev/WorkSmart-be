using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto
{
    public class ParsedCvData
    {
        // Thông tin cơ bản
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string JobPosition { get; set; }
        public string WorkType { get; set; }
        public string Summary { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Link { get; set; }

        // Các danh sách thông tin
        public List<ParsedEducation> Educations { get; set; } = new List<ParsedEducation>();
        public List<ParsedExperience> Experiences { get; set; } = new List<ParsedExperience>();
        public List<ParsedCertification> Certifications { get; set; } = new List<ParsedCertification>();
        public List<ParsedSkill> Skills { get; set; } = new List<ParsedSkill>();
    }

    public class ParsedEducation
    {
        public string Major { get; set; }
        public string SchoolName { get; set; }
        public string Degree { get; set; }
        public string Description { get; set; }
        public string StartedAt { get; set; }
        public string EndedAt { get; set; }
    }

    public class ParsedExperience
    {
        public string JobPosition { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string StartedAt { get; set; }
        public string EndedAt { get; set; }
    }

    public class ParsedCertification
    {
        public string CertificateName { get; set; }
        public string Description { get; set; }
        public string CreateAt { get; set; }
    }

    public class ParsedSkill
    {
        public string SkillName { get; set; }
        public string Description { get; set; }
    }
}
