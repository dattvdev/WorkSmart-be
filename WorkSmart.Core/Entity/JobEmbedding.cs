using System.ComponentModel.DataAnnotations;
using WorkSmart.Core.Helpers;

namespace WorkSmart.Core.Entity
{
    public class JobEmbedding
    {
        [Key]
        public int JobID { get; set; }
        public string VectorJson { get; set; }
        public DateTime UpdatedAt { get; set; } = TimeHelper.GetVietnamTime();
        public Job Job { get; set; }
    }
}
