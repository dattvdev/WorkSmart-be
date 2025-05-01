using System.ComponentModel.DataAnnotations;
using WorkSmart.Core.Helpers;

namespace WorkSmart.Core.Entity
{
    public class CVEmbedding
    {
        [Key]
        public int CVID { get; set; }
        public string VectorJson { get; set; }
        public DateTime UpdatedAt { get; set; } = TimeHelper.GetVietnamTime();
        public CV CV { get; set; }
    }
}