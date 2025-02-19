using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.CandidateDtos
{
    public class CandidateSearchRequestDto
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; }
        public string? Name { get; set; }
        public string? Education { get; set; }
        public string? Major { get; set; }
        public string? Skill { get; set; }
        public string? JobPosition { get; set; }
        public int Exp { get; set; } = 0;
        public string? WorkType { get; set; }
        public DateTime? LastUpdatedAt { get; set; }


    }
}
