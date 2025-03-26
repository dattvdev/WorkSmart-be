using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.PaymentDtos
{
    public class CancelPaymentDto
    {
        public long OrderCode { get; set; }
        public string? Reason { get; set; }
    }
}
