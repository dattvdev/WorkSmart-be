using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class AccountRepository : BaseRepository<User>, IAccountRepository
    {
        public AccountRepository(WorksmartDBContext context) : base(context)
        {

        }

        public User GetByEmail(string email)
        {
            return _dbSet.SingleOrDefault(x => x.Email == email);
        }

        public bool UserExist(string email)
        {
            return _dbSet.Any(x => x.Email == email);
        }
    }
}
