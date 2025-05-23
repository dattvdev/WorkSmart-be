﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.AccountDtos
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; }

        public string? ResetToken { get; set; }

        public string NewPassword { get; set; }
    }
}
