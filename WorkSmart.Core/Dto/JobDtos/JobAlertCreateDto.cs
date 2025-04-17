namespace WorkSmart.Core.Dto.JobDtos
{
    public class JobAlertCreateDto
    {
        public string Keyword { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Salary { get; set; }
        public string Experience { get; set; }
        public string WorkType { get; set; }
        public string Frequency { get; set; }
        public string NotifyVia { get; set; }
        public int UserId { get; set; }
    }

}
