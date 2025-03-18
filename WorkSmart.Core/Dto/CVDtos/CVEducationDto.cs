
namespace WorkSmart.Core.Dto.CVDtos
{
    public class CVEducationDto
    {
        public int EducationID { get; set; }
        public int? CVID { get; set; }
        public string? Major { get; set; }
        public string? SchoolName { get; set; }
        public string? Degree { get; set; }
        public string? Description { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }
}