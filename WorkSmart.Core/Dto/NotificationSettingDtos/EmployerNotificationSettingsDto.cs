using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.NotificationSettingDtos
{
    public class EmployerNotificationSettingsDto
    {
        public int? NotificationSettingID { get; set; }
        public int? UserID { get; set; }
        // Real-time Notifications
        public bool? NewApplications { get; set; } = true;
        public bool? ApplicationStatusUpdates { get; set; } = true;

        public bool? JobSubmission { get; set; } = true;
        public bool? JobApproved { get; set; } = true;
        public bool? JobRejected { get; set; } = true;

        public bool? WeeklyReports { get; set; } = true;
        public bool? PerformanceAlerts { get; set; } = true;

        // Email Notifications
        public bool? EmailNewApplications { get; set; } = true;
        public bool? EmailApplicationStatusUpdates { get; set; } = true;

        public bool? EmailJobSubmission { get; set; } = true;
        public bool? EmailJobApproved { get; set; } = true;
        public bool? EmailJobRejected { get; set; } = true;

        public bool? EmailWeeklyReports { get; set; } = true;
        public bool? EmailPerformanceAlerts { get; set; } = true;
    }

}
