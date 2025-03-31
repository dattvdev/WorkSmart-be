using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.PackageDtos;

namespace WorkSmart.Core.Dto.SubscriptionDtos
{
    public class SubscriptionWithPackage
    {
        public SubscriptionDto Subscription { get; set; }
        public GetPackageDto Package { get; set; }
    }
}
