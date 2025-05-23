﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Helpers;

namespace WorkSmart.Core.Entity
{
    public class PersonalMessage
    {
        public int PersonalMessageID { get; set; }
        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; } = TimeHelper.GetVietnamTime();
        public bool IsRead { get; set; } = false;

        public User Sender { get; set; }
        public User Receiver { get; set; }
    }
}
