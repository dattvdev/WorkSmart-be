using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Entity
{
    public class JobTag
    {
        public int JobTagID { get; set; }
        public string TagName { get; set; }

        public ICollection<NotificationJobTag>? NotificationJobTags { get; set; }
    }
}
    