using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Dto.UserDtos
{
    public class UserNotificationTagManageDto
    {
        public string? Category { get; set; }
        public List<EmailTag>? EmailTags { get; set; }
    }

    public class  EmailTag
    {
        public string? Email { get; set; }
        public List<Tag>? Tags { get; set; }

    }
}
