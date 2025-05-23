﻿using WorkSmart.Core.Helpers;

namespace WorkSmart.Core.Entity
{
    public class JobAlert
    {
        public int JobAlertId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public string Keyword { get; set; }
        public string Province { get; set; }
        public string? District { get; set; }
        public string SalaryRange { get; set; }
        public string? JobPosition { get; set; }
        public string? Experience { get; set; }
        public string? JobType { get; set; } 
        public string? Frequency { get; set; } // daily / weekly
        public string NotificationMethod { get; set; } // email / app / both

        public DateTime CreatedAt { get; set; } = TimeHelper.GetVietnamTime();
    }

}
