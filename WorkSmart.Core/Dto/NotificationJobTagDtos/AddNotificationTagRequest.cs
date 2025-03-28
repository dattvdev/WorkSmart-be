using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.NotificationJobTagDtos
{
    public class AddNotificationTagRequest
    {
        public int UserId { get; set; }
        public List<int> TagId { get; set; }
        public string Email { get; set; }
    }
}
