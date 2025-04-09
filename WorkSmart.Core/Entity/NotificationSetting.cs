using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Entity
{
    public class NotificationSetting
    {
        public int NotificationSettingID { get; set; }
        public int? UserID { get; set; }
        public User? User { get; set; } = null!;
        // ===============================
        // ===== Real-time Notifications
        // ===============================

        // Candidate
        public bool? SavedJobsUpdates { get; set; } = true;
        public bool? RecommendedJobs { get; set; } = true;

        // Candidate
        public bool? ApplicationApproved { get; set; } = true;
        public bool? ApplicationApply { get; set; } = true;
        public bool? ApplicationRejected { get; set; } = true;

        // Employer
        public bool? NewApplications { get; set; } = true;
        public bool? ApplicationStatusUpdates { get; set; } = true;

        // Employer
        public bool? JobSubmission { get; set; } = true;
        public bool? JobApproved { get; set; } = true;
        public bool? JobRejected { get; set; } = true;

        // Candidate
        public bool? ApplicationDeadlines { get; set; } = true;

        // Employer
        public bool? WeeklyReports { get; set; } = true;
        public bool? PerformanceAlerts { get; set; } = false;

        // ==========================
        // ===== Email Notifications
        // ==========================

        // Candidate
        public bool? EmailSavedJobsUpdates { get; set; } = true;
        public bool? EmailRecommendedJobs { get; set; } = true;

        // Candidate
        public bool? EmailApplicationApproved { get; set; } = true;
        public bool? EmailApplicationApply { get; set; } = true;
        public bool? EmailApplicationRejected { get; set; } = true;

        // Employer
        public bool? EmailNewApplications { get; set; } = true;
        public bool? EmailApplicationStatusUpdates { get; set; } = true;

        // Employer
        public bool? EmailJobSubmission { get; set; } = true;
        public bool? EmailJobApproved { get; set; } = true;
        public bool? EmailJobRejected { get; set; } = true;


        // Candidate
        public bool? EmailApplicationDeadlines { get; set; } = true;

        // Employer
        public bool? EmailWeeklyReports { get; set; } = true;
        public bool? EmailPerformanceAlerts { get; set; } = false;
    }


}
