﻿using System;
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
        public string? UserName { get; set; }
        public string? Avatar { get; set; }
        public string? IdentityNumber { get; set; }
        public bool IdentityConfirmed { get; set; } = false;
        public string Email { get; set; } //candidate and employer
        public string Role { get; set; } //candidate and employer and admin
        public string PasswordHash { get; set; } //candidate and employer
        public string? ConfirmationCode { get; set; }
        public bool IsEmailConfirmed { get; set; } //candidate and employer
        public bool IsBanned { get; set; } = false;
        public bool IsPrivated { get; set; } = false;
        public double Amount { get; set; } = 0;
        public string? BankName { get; set; }
        public string? BankNumber { get; set; }
        public string FullName { get; set; } //candidate and employer
        public string? PhoneNumber { get; set; } //employer
        public string? Gender { get; set; } //employer & candidate
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; } // Candidate address || Company Location
        public string? CompanyName { get; set; } //employer
        public string? CompanyDescription { get; set; }
        public string? WorkLocation {  get; set; } //employer
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public int VerificationLevel { get; set; } = 1; // Cấp độ xác thực 1/2/3
        public string? TaxId { get; set; } // Mã số thuế
        public string? Industry { get; set; } // Lĩnh vực hoạt động
        public string? CompanySize { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? TaxVerificationStatus { get; set; } = "Active"; // Pending, Approved, Rejected
        public string? TaxVerificationReason { get; set; } // Lý do từ chối
        public string? BusinessLicenseImage { get; set; }
        public string? LicenseVerificationStatus { get; set; } = "Active"; // Pending, Approved, Rejected
        public string? LicenseVerificationReason { get; set; } // Lý do từ chối

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
        public ICollection<NotificationJobTag>? NotificationJobTags { get; set; }
        public ICollection<Tag>? Tags { get; set; }
    }
}
