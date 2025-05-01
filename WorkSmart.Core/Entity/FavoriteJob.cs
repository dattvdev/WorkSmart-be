using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Helpers;

namespace WorkSmart.Core.Entity
{
    public class FavoriteJob
    {

        public int FavoriteJobID { get; set; }
        public int UserID { get; set; }
        public int JobID { get; set; }
        public DateTime CreateAt { get; set; } = TimeHelper.GetVietnamTime();

        public User User { get; set; }
        public Job Job { get; set; }
    }
}
