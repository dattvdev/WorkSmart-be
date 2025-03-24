using System.Text.Json.Serialization;

namespace WorkSmart.Core.Entity
{
    public class CV
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
        public bool? IsHidden { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public User User { get; set; }
        public CV_Template? CVTemplate { get; set; }
        public ICollection<Application>? Applications { get; set; }
        [JsonIgnore]
        public ICollection<CV_Education>? Educations { get; set; }
        public ICollection<CV_Experience>? Experiences { get; set; }
        public ICollection<CV_Certification>? Certifications { get; set; }
        public ICollection<CV_Skill>? Skills { get; set; }
    }
}
