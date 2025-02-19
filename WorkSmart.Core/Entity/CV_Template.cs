using System.ComponentModel.DataAnnotations;

namespace WorkSmart.Core.Entity
{
    public class CV_Template
    {
        [Key]
        public int CVTemplateId { get; set; }
        public string Name { get; set; } 
        public string Thumbnail { get; set; } 
        public string FilePath { get; set; }
        public ICollection<CV>? CVs { get; set; }
    }
}
