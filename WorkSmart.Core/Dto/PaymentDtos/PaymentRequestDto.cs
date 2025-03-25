using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.PaymentDtos
{
    public class PaymentRequestDto
    {
        public int PackageId { get; set; }
        public int UserId { get; set; }
    }
}
