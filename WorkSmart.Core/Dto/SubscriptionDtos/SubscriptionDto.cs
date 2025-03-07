using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.SubscriptionDtos
{
    public class SubscriptionDto
    {
        public int PackageID { get; set; }
        public int UserID { get; set; }
        public DateTime ExpDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now; 
    }
}
