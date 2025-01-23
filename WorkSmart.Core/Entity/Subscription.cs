using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Entity
{
    public class Subscription
    {
        public int SubscriptionID { get; set; }
        public int PackageID { get; set; }
        public int UserID { get; set; }
        public DateTime ExpDate { get; set; }

        public User User { get; set; }
        public Package Package { get; set; }
    }

}
