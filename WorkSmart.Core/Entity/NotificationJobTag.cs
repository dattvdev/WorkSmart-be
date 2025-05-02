using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Helpers;

namespace WorkSmart.Core.Entity
{
    public class NotificationJobTag
    {
        public int NotificationJobTagID { get; set; }
        public int TagID { get; set; }
        public int UserID { get; set; }
        public string Email { get; set; }
        public DateTime CreateAt { get; set; } = TimeHelper.GetVietnamTime();

        public Tag Tag { get; set; }
        public User User { get; set; }
    }
}
