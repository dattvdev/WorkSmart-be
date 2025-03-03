using Microsoft.EntityFrameworkCore;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class CVRepository : BaseRepository<CV>, ICVRepository
    {
        public CVRepository(WorksmartDBContext context) : base(context) { }

        public async Task<IEnumerable<CV>> GetAllByUserIdAsync(int userId)
        {
            return await _dbSet.Where(c => c.UserID == userId).ToListAsync();
        }
    }
}