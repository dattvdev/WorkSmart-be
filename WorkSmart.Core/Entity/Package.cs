using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Entity
{
    public class Package
    {
        public int PackageID { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }

        public ICollection<Subscription>? Subscriptions { get; set; }
    }
}
