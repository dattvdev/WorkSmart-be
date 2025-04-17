using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface IJobAlertRepository : IBaseRepository<JobAlert>
    {
        Task<IEnumerable<JobAlert>> GetJobAlertsByUserId(int userId);
    }
}
