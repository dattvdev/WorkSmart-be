using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Helpers;

namespace WorkSmart.Core.Dto.NotificationDtos
{
    public class CreateNotificationDto
    {
        public int UserID { get; set; }
        public string Title { get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; } = false;
        public string? Link { get; set; }
        public DateTime CreatedAt { get; set; } = TimeHelper.GetVietnamTime();
    }
}
