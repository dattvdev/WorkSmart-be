using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.NotificationSettingDtos
{
    public class CandidateNotificationSettingsUpdateDto
    {
         // Real-time Notifications
        public bool? NewJobMatches { get; set; }
        public bool? SavedJobsUpdates { get; set; }
        public bool? RecommendedJobs { get; set; }

        public bool? ApplicationReviewed { get; set; }
        public bool? InterviewInvitation { get; set; }
        public bool? ApplicationRejected { get; set; }

        public bool? MessagesReceived { get; set; }
        public bool? ProfileViews { get; set; }

        public bool? UpcomingInterviews { get; set; }
        public bool? ApplicationDeadlines { get; set; }
        public bool? CareerEvents { get; set; }

        // Email Notifications
        public bool? EmailNewJobMatches { get; set; }
        public bool? EmailSavedJobsUpdates { get; set; }
        public bool? EmailRecommendedJobs { get; set; }

        public bool? EmailApplicationReviewed { get; set; }
        public bool? EmailInterviewInvitation { get; set; }
        public bool? EmailApplicationRejected { get; set; }

        public bool? EmailMessagesReceived { get; set; }
        public bool? EmailProfileViews { get; set; }

        public bool? EmailUpcomingInterviews { get; set; }
        public bool? EmailApplicationDeadlines { get; set; }
        public bool? EmailCareerEvents { get; set; }
    }
}
