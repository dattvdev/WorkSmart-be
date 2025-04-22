using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User> GetUserById(int id);
        Task<User> GetEmployerByCompanyName(string companyName);
        Task<(IEnumerable<User>, int total)> GetListCompany(string? searchName, int page, int pageSize);
        Task<IEnumerable<object>> UserDashboard();
        Task<IEnumerable<object>> CountDashboard();
        Task<IEnumerable<User>> GetUsersWithFeaturedCV();
    }
}
