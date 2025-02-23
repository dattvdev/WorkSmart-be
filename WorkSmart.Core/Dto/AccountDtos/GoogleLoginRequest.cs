using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.AccountDtos
{
    public class GoogleLoginRequest
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string? Role { get; set; }
    }
}
