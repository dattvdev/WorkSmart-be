using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface ISubscriptionRepository : IBaseRepository<Subscription>
    {
        Task<Subscription> GetByUserId(int id);
    }
}
