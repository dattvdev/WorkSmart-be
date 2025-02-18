using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface ICandidateRepository : IBaseRepository<User>
    {
        Task<IEnumerable<User>> GetListSearch(int pageIndex, int pageSize, 
            double? exp = null, List<int>? tagIds = null,
            string? address = null);
    }
}
