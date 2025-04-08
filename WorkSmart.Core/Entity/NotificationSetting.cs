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
        public bool? NewJobMatches { get; set; } = true;
        public bool? SavedJobsUpdates { get; set; } = true;
        public bool? RecommendedJobs { get; set; } = true;

        // Candidate
        public bool? ApplicationReviewed { get; set; } = true;
        public bool? InterviewInvitation { get; set; } = true;
        public bool? ApplicationRejected { get; set; } = true;

        // Employer
        public bool? NewApplications { get; set; } = true;
        public bool? ApplicationStatusUpdates { get; set; } = true;

        // Employer
        public bool? JobSubmission { get; set; } = true;
        public bool? JobApproved { get; set; } = true;
        public bool? JobRejected { get; set; } = true;

        // Both Candidate & Employer
        public bool? MessagesReceived { get; set; } = false;

        // Candidate
        public bool? UpcomingInterviews { get; set; } = true;
        public bool? ApplicationDeadlines { get; set; } = true;
        public bool? CareerEvents { get; set; } = true;

        // Both Candidate & Employer
        public bool? ProfileViews { get; set; } = false;

        // Employer
        public bool? WeeklyReports { get; set; } = true;
        public bool? PerformanceAlerts { get; set; } = false;

        // ==========================
        // ===== Email Notifications
        // ==========================

        // Candidate
        public bool? EmailNewJobMatches { get; set; } = true;
        public bool? EmailSavedJobsUpdates { get; set; } = true;
        public bool? EmailRecommendedJobs { get; set; } = true;

        // Candidate
        public bool? EmailApplicationReviewed { get; set; } = true;
        public bool? EmailInterviewInvitation { get; set; } = true;
        public bool? EmailApplicationRejected { get; set; } = true;

        // Employer
        public bool? EmailNewApplications { get; set; } = true;
        public bool? EmailApplicationStatusUpdates { get; set; } = true;

        // Employer
        public bool? EmailJobSubmission { get; set; } = true;
        public bool? EmailJobApproved { get; set; } = true;
        public bool? EmailJobRejected { get; set; } = true;

        // Both
        public bool? EmailMessagesReceived { get; set; } = false;

        // Candidate
        public bool? EmailUpcomingInterviews { get; set; } = true;
        public bool? EmailApplicationDeadlines { get; set; } = true;
        public bool? EmailCareerEvents { get; set; } = true;

        // Both
        public bool? EmailProfileViews { get; set; } = false;

        // Employer
        public bool? EmailWeeklyReports { get; set; } = true;
        public bool? EmailPerformanceAlerts { get; set; } = false;
    }


}
