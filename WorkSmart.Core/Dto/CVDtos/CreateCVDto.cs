namespace WorkSmart.Core.Dto.CVDtos
{
    public class CreateCVDto
    {
        public int CVTemplateId { get; set; }
        public string FullName { get; set; }
        public string? JobPosition { get; set; }
        public string? WorkType { get; set; }
        public string? Summary { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Link { get; set; }

        public List<CreateCVEducationDto> Educations { get; set; } = new();
        public List<CreateCVExperienceDto> Experiences { get; set; } = new();
        public List<CreateCVSkillDto> Skills { get; set; } = new();
    }

    public class CreateCVEducationDto
    {
        public string Major { get; set; }
        public string SchoolName { get; set; }
        public string? Degree { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }

    public class CreateCVExperienceDto
    {
        public string JobPosition { get; set; }
        public string CompanyName { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }

    public class CreateCVSkillDto
    {
        public string SkillName { get; set; }
    }
}
