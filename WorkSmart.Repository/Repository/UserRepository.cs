using Microsoft.EntityFrameworkCore;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;
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
            var user = await _dbSet
                .Include(l => l.PostedJobs)
                .FirstOrDefaultAsync(x => x.CompanyName == companyName);

            if (user != null)
            {
                // Lọc PostedJobs chỉ giữ lại các công việc có status là Active
                user.PostedJobs = user.PostedJobs.Where(job => job.Status == JobStatus.Active).ToList();
                return user;
            }

            return new User();
        }

        public async Task<(IEnumerable<User>, int total)> GetListCompany(string? searchName, int page, int pageSize)
        {
            // Lấy danh sách người dùng với vai trò Employer và đã được xác minh cấp độ 3
            var users = await _dbSet
                .Where(x => searchName == null || x.CompanyName.Contains(searchName))
                .Where(x => x.Role == "Employer" && x.VerificationLevel == 3)
                .Include(x => x.PostedJobs)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lọc PostedJobs chỉ giữ lại các công việc có status = JobStatus.Active (3)
            foreach (var user in users)
            {
                user.PostedJobs = user.PostedJobs.Where(job => job.Status == JobStatus.Active).ToList();
            }

            var totalRecord = await _dbSet
                .CountAsync(x => (searchName == null || x.CompanyName.Contains(searchName))
                                 && x.Role == "Employer" && x.VerificationLevel == 3);

            return (users, totalRecord);
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

        public async Task<User> GetUserById(int id)
        {
            return await _dbSet.Include(u => u.NotificationSetting).Where(u => u.UserID == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetUsersWithFeaturedCV()
        {
            return await _context.Users
                .Include(u => u.CVs)
                    .ThenInclude(cv => cv.Educations)
                .Include(u => u.CVs)
                    .ThenInclude(cv => cv.Experiences)
                .Include(u => u.CVs)
                    .ThenInclude(cv => cv.Certifications)
                .Include(u => u.CVs)
                    .ThenInclude(cv => cv.Skills)
                .Where(u => u.Role != "Admin" && u.CVs.Any(cv => cv.IsFeatured == true) && !u.IsPrivated)
                .ToListAsync();
        }
    }
}
