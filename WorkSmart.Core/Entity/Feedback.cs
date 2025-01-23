using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Entity
{
    public class Feedback
    {
        public int FeedbackID { get; set; }
        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public string? Content { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User Sender { get; set; }
        public User Receiver { get; set; }
    }

}
