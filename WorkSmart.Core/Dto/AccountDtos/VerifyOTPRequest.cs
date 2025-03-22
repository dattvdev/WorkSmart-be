using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.AccountDtos
{
    public class VerifyOTPRequest
    {
        public string Email { get; set; }

        public string Otp { get; set; }
    }
}
