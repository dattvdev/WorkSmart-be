using System.ComponentModel.DataAnnotations;

namespace WorkSmart.Core.Entity
{
    public class CVEmbedding
    {
        [Key]
        public int CVID { get; set; }
        public string VectorJson { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public CV CV { get; set; }
    }
}