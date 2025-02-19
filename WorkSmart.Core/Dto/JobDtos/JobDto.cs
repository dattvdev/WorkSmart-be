namespace WorkSmart.Core.Dto.JobDtos
{
    public class JobDto
    {
        public int JobID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public double? Salary { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
