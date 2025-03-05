using System.Text.Json.Serialization;

namespace WorkSmart.Core.Dto.CVDtos
{
    public class CVExperienceDto
    {
        public int ExperienceID { get; set; }
        public int CVID { get; set; }
        public string? JobPosition { get; set; }
        public string? CompanyName { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }

}
