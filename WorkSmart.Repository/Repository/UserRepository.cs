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

        public async Task<IEnumerable<object>> UserDashboard()
        {
            var userCountsByMonth = await _context.Users
                .Where(u => u.CreatedAt.Year == DateTime.Now.Year) // Chỉ lấy dữ liệu năm hiện tại
                .GroupBy(u => new { u.CreatedAt.Month }) // Nhóm theo tháng
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Employers = g.Count(u => u.Role == "employer"),
                    JobSeekers = g.Count(u => u.Role == "candidate"),
                    Total = g.Count()
                })
                .ToListAsync();

            // Danh sách 12 tháng mặc định
            var months = Enumerable.Range(1, 12).Select(m => new
            {
                Month = m,
                Employers = 0,
                JobSeekers = 0,
                Total = 0
            }).ToList();

            // Ghép dữ liệu thực tế vào danh sách mặc định
            foreach (var item in userCountsByMonth)
            {
                var index = item.Month - 1;
                months[index] = item;
            }

            // Chuyển đổi số tháng thành tên tháng
            var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            var result = months.Select(m => new
            {
                Month = monthNames[m.Month - 1],
                Employers = m.Employers,
                JobSeekers = m.JobSeekers,
                Total = m.Total
            }).ToList();

            return result;
        }
        public async Task<IEnumerable<object>> CountDashboard()
        {
            var postedJobs = await _context.Jobs.CountAsync();
            var users = await _context.Users.CountAsync();
            var messages = await _context.PersonalMessages.CountAsync();
            var revenue = await _context.Subscriptions
                                 .Include(s => s.Package)
                                 .SumAsync(s => s.Package.Price);

            var result = new List<object>
            {
                new { Title = "Posted Jobs", Count = postedJobs },
                new { Title = "Users", Count = users },
                new { Title = "Messages", Count = messages },
                new { Title = "Revenue", Count = revenue }
            };

            return result;
        }

    }
}
