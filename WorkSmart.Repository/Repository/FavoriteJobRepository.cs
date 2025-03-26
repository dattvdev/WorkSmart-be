using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class FavoriteJobRepository : BaseRepository<FavoriteJob>, IFavoriteJobRepository
    {
        public FavoriteJobRepository(WorksmartDBContext context) : base(context)
        {

        }
    }
}
