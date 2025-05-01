using System.Text.Json.Serialization;
using WorkSmart.Core.Helpers;

namespace WorkSmart.Core.Dto.CVDtos
{
    public class CVDto
    {
        public int CVID { get; set; }
        public int UserID { get; set; }
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
        public bool? IsFeatured { get; set; } = false;
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public bool? IsHidden { get; set; } = false;
        public DateTime CreatedAt { get; set; } = TimeHelper.GetVietnamTime();
        public DateTime UpdatedAt { get; set; } = TimeHelper.GetVietnamTime();
        public ICollection<CVExperienceDto>? Experiences { get; set; }
        public ICollection<CVCertificationDto>? Certifications { get; set; }
        public ICollection<CVSkillDto>? Skills { get; set; }
        public ICollection<CVEducationDto>? Educations { get; set; }
    }
}
