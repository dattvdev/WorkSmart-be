using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface ICVRepository : IBaseRepository<CV>
    {
        Task<IEnumerable<CV>> GetAllByUserIdAsync(int userId);
        Task<CV> CreateCVAsync(CV cv);
    }

}
