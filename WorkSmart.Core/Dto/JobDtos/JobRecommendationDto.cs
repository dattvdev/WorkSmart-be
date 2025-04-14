using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Dto.JobDtos
{
    public class JobRecommendationDto
    {
        public JobDto Job { get; set; }
        public float Score { get; set; }
    }

}
