using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.JobDtos
{
    public class JobInvitationRequestDto
    {
        public int CandidateId { get; set; }
        public int EmployerId { get; set; }
        public int JobId { get; set; }
    }
}
