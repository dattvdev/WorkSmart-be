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

        public async Task<(IEnumerable<User>, int total)> GetListCompany(string? searchName, int page, int pageSize)
        {
            //return list and total record check searchName null
            var list = await _dbSet
                .Where(x => searchName == null || x.CompanyName.Contains(searchName))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var totalRecord = await _dbSet.CountAsync(x => searchName == null || x.CompanyName.Contains(searchName));
            return (list, totalRecord);
        }
    }
}
