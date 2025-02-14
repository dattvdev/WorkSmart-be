using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Entity
{
    public class NotificationJobTag
    {
        public int NotificationJobTagID { get; set; }
        public int JobTagID { get; set; }
        public int UserID { get; set; }
        public string Email { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;

        public JobTag JobTag { get; set; }
        public User User { get; set; }
    }
}
