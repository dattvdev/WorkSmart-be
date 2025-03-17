using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Entity
{
    public class Tag
    {
        public int TagID { get; set; }
        public string TagName { get; set; }
        public int CategoryID { get; set; }

        public ICollection<NotificationJobTag>? NotificationJobTags { get; set; }
        public ICollection<Job>? Jobs { get; set; }
        public ICollection<User>? Users { get;}
    }
}
    