using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.NotificationDtos
{
    public class CandidateNotificationsDto
    {
        public int NotificationId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public string NotificationType { get; set; } // For filtering (SavedJobsUpdates, RecommendedJobs, etc.)
        public int? RelatedJobId { get; set; } // If notification is related to a specific job
        public string Title { get; set; }
    }
}
