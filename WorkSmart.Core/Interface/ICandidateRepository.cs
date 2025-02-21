using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface ICandidateRepository : IBaseRepository<User>
    {
        Task<(IEnumerable<User> Users, int Total)> GetListSearch(CandidateSearchRequestDto request);
    }
}
