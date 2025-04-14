using System.ComponentModel.DataAnnotations;

namespace WorkSmart.Core.Entity
{
    public class JobEmbedding
    {
        [Key]
        public int JobID { get; set; }
        public string VectorJson { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public Job Job { get; set; }
    }
}
