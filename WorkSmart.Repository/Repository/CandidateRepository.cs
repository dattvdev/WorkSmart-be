using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class CandidateRepository : BaseRepository<User>, ICandidateRepository
    {
        public CandidateRepository(WorksmartDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<User>> GetListSearch(CandidateSearchRequestDto request)
        {
            /*var query = _dbSet.AsQueryable();

            if (exp.HasValue)
            {
                query = query.Where(c => c.Exp >= exp.Value);
            }

            if (!string.IsNullOrEmpty(address))
            {
                query = query.Where(c => c.Address.Contains(address));
            }

            if (tagIds != null && tagIds.Any())
            {
                query = query.Where(c => c.Tags.Any(t => tagIds.Contains(t.TagID)));
            }
            var candidates = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return candidates;*/
            return null;
        }
    }
}
