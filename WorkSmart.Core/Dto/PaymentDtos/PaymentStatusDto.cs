using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.PaymentDtos
{
    public class PaymentStatusDto
    {
        public long OrderCode { get; set; }
        public string Status { get; set; }
        public double Price { get; set; }
        public string Content { get; set; }
    }
}
