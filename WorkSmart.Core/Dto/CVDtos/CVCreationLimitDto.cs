using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.CVDtos
{
    public class CVCreationLimitDto
    {
        public int RemainingLimit {  get; set; }
        public int TotalLimit { get; set; }
        public int UsedTotal {  get; set; }
        public string Message { get; set; }
        public string PackageName { get; set; }
    }
}
