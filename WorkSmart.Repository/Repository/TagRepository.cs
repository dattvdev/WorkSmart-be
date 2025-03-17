using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class TagRepository : BaseRepository<Tag>, ITagRepository
    {
        public TagRepository(WorksmartDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Tag>> GetByCategory(string category)
        {
            if (category == null) return new List<Tag>();
            return await _dbSet.Where(t => t.CategoryID == category).ToListAsync();
        }
    }
}
