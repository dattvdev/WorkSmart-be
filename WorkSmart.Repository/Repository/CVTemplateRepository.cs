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
    public class CVTemplateRepository : BaseRepository<CV_Template>, ICVTemplateRepository
    {
        public CVTemplateRepository(WorksmartDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CV_Template>> GetTemplatesByNameAsync(string name)
        {
            return await _dbSet
                .Where(t => t.Name.Contains(name))
                .ToListAsync();
        }

        Task<IEnumerable<CV_Template>> ICVTemplateRepository.GetTemplatesByNameAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}
