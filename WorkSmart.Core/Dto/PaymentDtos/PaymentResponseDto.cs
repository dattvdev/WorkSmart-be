using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.PaymentDtos
{
    public class PaymentResponseDto
    {
        public string CheckoutUrl { get; set; }
        public long OrderCode { get; set; }
        public double Amount { get; set; }
        public string PackageName { get; set; }
    }
}
