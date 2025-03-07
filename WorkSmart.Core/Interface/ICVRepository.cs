using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface ICVRepository : IBaseRepository<CV>
    {
        Task<IEnumerable<CV>> GetAllCVsByUserId(int userId);
        Task<CV> GetCVWithDetails(int id);
        void Update(CV cv); 
        Task Delete(int id);
        void SetFeature(int cvId, int userId);
    }

}
