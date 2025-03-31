using Microsoft.EntityFrameworkCore;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class FavoriteJobRepository : BaseRepository<FavoriteJob>, IFavoriteJobRepository
    {
        public FavoriteJobRepository(WorksmartDBContext context) : base(context)
        {

        }

        public async Task<IEnumerable<FavoriteJob>> GetByUserIdAsync(int userId)
        {
            return await _context.FavoriteJobs.Where(fj => fj.UserID == userId).ToListAsync();
        }
    }
}
