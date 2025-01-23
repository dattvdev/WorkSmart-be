using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Transactions;

namespace WorkSmart.Core.Entity
{
    public class User
    {
        public int UserID { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string PasswordHash { get; set; }
        public bool IsBanned { get; set; } = false;
        public double Amount { get; set; } = 0;
        public string? BankName { get; set; }
        public string? BankNumber { get; set; }
        public string? FullName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public double? Exp { get; set; }
        public string? Skills { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyDescription { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Notification>? Notifications { get; set; }
        public ICollection<CV>? CVs { get; set; }
        public ICollection<FavoriteJob>? FavoriteJobs { get; set; }
        public ICollection<Application>? Applications { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
        public ICollection<Subscription>? Subscriptions { get; set; }
        public ICollection<Feedback>? FeedbacksSent { get; set; }
        public ICollection<Feedback>? FeedbacksReceived { get; set; }
        public ICollection<ReportUser>? ReportsSent { get; set; }
        public ICollection<ReportUser>? ReportsReceived { get; set; }
        public ICollection<PersonalMessage>? MessagesSent { get; set; }
        public ICollection<PersonalMessage>? MessagesReceived { get; set; }
        public ICollection<Job>? PostedJobs { get; set; }
        public ICollection<ReportPost>? ReportPosts { get; set; }
        public ICollection<JobTag>? JobTags { get; set; }

    }
}
