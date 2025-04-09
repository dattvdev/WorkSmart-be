using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.NotificationSettingDtos
{
    public class CandidateNotificationSettingsDto
    {
        public int? NotificationSettingID { get; set; }
        public int? UserID { get; set; }
        // Real-time Notifications
        public bool? NewJobMatches { get; set; } = true;
        public bool? SavedJobsUpdates { get; set; } = true;
        public bool? RecommendedJobs { get; set; } = true;

        public bool? ApplicationReviewed { get; set; } = true;
        public bool? InterviewInvitation { get; set; } = true;
        public bool? ApplicationRejected { get; set; } = true;

        public bool? MessagesReceived { get; set; } = true;
        public bool? ProfileViews { get; set; } = true;

        public bool? UpcomingInterviews { get; set; } = true;
        public bool? ApplicationDeadlines { get; set; } = true;
        public bool? CareerEvents { get; set; } = true;

        // Email Notifications
        public bool? EmailNewJobMatches { get; set; } = true;
        public bool? EmailSavedJobsUpdates { get; set; } = true;
        public bool? EmailRecommendedJobs { get; set; } = true;

        public bool? EmailApplicationReviewed { get; set; } = true;
        public bool? EmailInterviewInvitation { get; set; } = true;
        public bool? EmailApplicationRejected { get; set; } = true;

        public bool? EmailMessagesReceived { get; set; } = true;
        public bool? EmailProfileViews { get; set; } = true;

        public bool? EmailUpcomingInterviews { get; set; } = true;
        public bool? EmailApplicationDeadlines { get; set; } = true;
        public bool? EmailCareerEvents { get; set; } = true;
    }

}
