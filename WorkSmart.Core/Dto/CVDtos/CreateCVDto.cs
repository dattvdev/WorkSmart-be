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
    }

    public class CVDto
    {
        public int CVID { get; set; }
        public int UserID { get; set; }
        public int CVTemplateId { get; set; }
        public string FullName { get; set; }
        public string? JobPosition { get; set; }
        public string? WorkType { get; set; }
        public string? Summary { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Link { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
