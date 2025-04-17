namespace WorkSmart.Core.Entity
{
    public class JobAlert
    {
        public int JobAlertId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public string Keyword { get; set; }
        public string City { get; set; }
        public string District { get; set; }

        public string SalaryLevel { get; set; }
        public string Experience { get; set; }
        public string JobPosition { get; set; }
        public string WorkType { get; set; }

        public string Frequency { get; set; }
        public string NotifyBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
