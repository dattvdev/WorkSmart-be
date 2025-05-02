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

        public async Task<bool> IsJobFavoritedAsync(int userId, int jobId)
        {
            return await _context.FavoriteJobs
                .AnyAsync(fj => fj.UserID == userId && fj.JobID == jobId);
        }
        public void DeleteFavoriteJob(int userId, int jobId)
        {
            var favoriteJob = _context.FavoriteJobs
                .FirstOrDefault(fj => fj.UserID == userId && fj.JobID == jobId);
            if (favoriteJob != null)
            {
                _context.FavoriteJobs.Remove(favoriteJob);
                _context.SaveChanges();
            }
        }
    }
}
