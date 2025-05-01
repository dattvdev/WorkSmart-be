using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Helpers;

namespace WorkSmart.Core.Dto.CVDtos
{
    public class CvUploadDto
    {
        public int CVID { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public bool? IsHidden { get; set; } = false;
        public DateTime CreatedAt { get; set; } = TimeHelper.GetVietnamTime();
    }
}
