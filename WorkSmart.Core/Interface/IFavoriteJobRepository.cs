using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface IFavoriteJobRepository : IBaseRepository<FavoriteJob>
    {
        Task<IEnumerable<FavoriteJob>> GetByUserIdAsync(int userId);
        Task<bool> IsJobFavoritedAsync(int userId, int jobId);
        void DeleteFavoriteJob(int userId, int jobId);
    }
}
