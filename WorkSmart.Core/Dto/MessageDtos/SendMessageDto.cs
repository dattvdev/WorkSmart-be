using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.MessageDtos
{
    public class SendMessageDto
    {
        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public string Content { get; set; }
    }
}
