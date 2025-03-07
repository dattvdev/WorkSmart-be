using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.MessageDtos
{
    public class ConversationUserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        public string LastMessage { get; set; }
    }
}
