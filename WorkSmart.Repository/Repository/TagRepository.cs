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

        //public async Task<IEnumerable<Tag>> GetTagByListID(List<int> tagID)
        //{
        //    var tags = await _dbSet
        //    .Where(t => tagID.Contains(t.TagID))
        //    .ToListAsync();
        //    return tags;
        //}
        public async Task<IEnumerable<Tag>> GetTagByListID(List<int> tagID)
        {
            if (tagID == null || !tagID.Any())
            {
                return new List<Tag>();
            }

            return await _dbSet
                .Where(t => tagID.Contains(t.TagID))
                .ToListAsync();
        }
    }
}
