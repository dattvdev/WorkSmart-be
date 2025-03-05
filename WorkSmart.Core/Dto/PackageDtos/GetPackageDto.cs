using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Dto.PackageDtos
{
    public class GetPackageDto
    {
        public int PackageID { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }

        public ICollection<Subscription>? Subscriptions { get; set; }
    }
}
