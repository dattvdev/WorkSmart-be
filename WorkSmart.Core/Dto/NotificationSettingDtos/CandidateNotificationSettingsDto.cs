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
        public bool? SavedJobsUpdates { get; set; } = true;
        public bool? RecommendedJobs { get; set; } = true;

        public bool? ApplicationApproved { get; set; } = true;
        public bool? ApplicationApply { get; set; } = true;
        public bool? ApplicationRejected { get; set; } = true;

        public bool? ApplicationDeadlines { get; set; } = true;

        // Email Notifications
        public bool? EmailSavedJobsUpdates { get; set; } = true;
        public bool? EmailRecommendedJobs { get; set; } = true;

        public bool? EmailApplicationApproved { get; set; } = true;
        public bool? EmailApplicationApply { get; set; } = true;
        public bool? EmailApplicationRejected { get; set; } = true;

        public bool? EmailApplicationDeadlines { get; set; } = true;
    }

}
