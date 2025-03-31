using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface IFavoriteJobRepository : IBaseRepository<FavoriteJob>
    {
        Task<IEnumerable<FavoriteJob>> GetByUserIdAsync(int userId);
    }
}
