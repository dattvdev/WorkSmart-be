using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Entity
{
    public class ReportUser
    {
        public int ReportUserID { get; set; }
        public int SenderID { get; set; }
        public int ReceiverID { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }

        public User Sender { get; set; }
        public User Receiver { get; set; }
    }

}
