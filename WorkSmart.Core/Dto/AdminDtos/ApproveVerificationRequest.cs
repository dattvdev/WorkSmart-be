using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.AdminDtos
{
    public class ApproveVerificationRequest
    {
        public bool IsApproved { get; set; }
        public string? Reason { get; set; }
    }
}
