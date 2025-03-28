using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.PaymentDtos
{
    public class PayOSSettings
    {
        public string ClientId { get; set; }
        public string ApiKey { get; set; }
        public string ChecksumKey { get; set; }
    }
}
