using Microsoft.EntityFrameworkCore;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(WorksmartDBContext context) : base(context)
        {

        }

        public async Task<User> GetEmployerByCompanyName(string companyName)
        {
            var user = await _dbSet.Include(l => l.PostedJobs).FirstOrDefaultAsync(x => x.CompanyName == companyName);
            if (user != null)
            {
                return user;
            }
            return new User();
        }
    }
}
