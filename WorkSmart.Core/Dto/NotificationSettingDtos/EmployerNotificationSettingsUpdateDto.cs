using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.NotificationSettingDtos
{
    public class EmployerNotificationSettingsUpdateDto
    {
        // Real-time Notifications
        public bool? NewApplications { get; set; } 
        public bool? ApplicationStatusUpdates { get; set; } 

        public bool? JobSubmission { get; set; } 
        public bool? JobApproved { get; set; } 
        public bool? JobRejected { get; set; } 

        public bool? MessagesReceived { get; set; } 
        public bool? ProfileViews { get; set; } 

        public bool? WeeklyReports { get; set; } 
        public bool? PerformanceAlerts { get; set; } 

        // Email Notifications
        public bool? EmailNewApplications { get; set; } 
        public bool? EmailApplicationStatusUpdates { get; set; } 

        public bool? EmailJobSubmission { get; set; } 
        public bool? EmailJobApproved { get; set; } 
        public bool? EmailJobRejected { get; set; } 

        public bool? EmailMessagesReceived { get; set; } 
        public bool? EmailProfileViews { get; set; } 

        public bool? EmailWeeklyReports { get; set; } 
        public bool? EmailPerformanceAlerts { get; set; } 
    }
}
