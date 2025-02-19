using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.CandidateDtos
{
    public class CandidateSearchRequestDto
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public double? Exp { get; set; }
        public List<int>? TagIds { get; set; }
        public string? Address { get; set; }
    }
}
