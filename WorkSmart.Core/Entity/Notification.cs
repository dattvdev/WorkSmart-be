﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Helpers;

namespace WorkSmart.Core.Entity
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public int UserID { get; set; }
        public string Title { get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; } = false;
        public string? Link { get; set; }
        public DateTime CreatedAt { get; set; } = TimeHelper.GetVietnamTime();

        public User User { get; set; }

    }
}
