using System.ComponentModel.DataAnnotations;

namespace WorkSmart.Core.Entity
{
    public class CV_Skill
    {
        [Key]
        public int SkillID { get; set; }
        public int CVID { get; set; }
        public string? SkillName { get; set; }
        public string? Description { get; set; }
        public CV CV { get; set; }
    }
}
