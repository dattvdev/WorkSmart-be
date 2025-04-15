using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.CVDtos
{
    public class FeaturedCVDto
    {
        public int CVID { get; set; }
        public int UserID { get; set; }
        public int? CVTemplateId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? JobPosition { get; set; }
        public string? WorkType { get; set; }
        public string? Summary { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Link { get; set; }
        public string? Borderstyle { get; set; }
        public string? Colorhex { get; set; }
        public bool? IsFeatured { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public bool? IsHidden { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Related entities
        public List<CVEducationDto>? Educations { get; set; }
        public List<CVExperienceDto>? Experiences { get; set; }
        public List<CVCertificationDto>? Certifications { get; set; }
        public List<CVSkillDto>? Skills { get; set; }
    }
}
