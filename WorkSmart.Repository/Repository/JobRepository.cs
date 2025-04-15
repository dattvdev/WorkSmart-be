using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;
using WorkSmart.Core.Interface;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
namespace WorkSmart.Repository.Repository
{
    public class JobRepository : BaseRepository<Job>, IJobRepository
    {
        private readonly WorksmartDBContext _context;
   

        public JobRepository(WorksmartDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> ApproveJobAsync(int jobId)
        {
            var job = await _dbSet.FindAsync(jobId);
            if (job == null) return false;

            job.Status = JobStatus.Active;
            //job.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RejectJobAsync(int jobId, string reason)
        {
            var job = await _dbSet.FindAsync(jobId);
            if (job == null) return false;

            job.Status = JobStatus.Rejected;
            job.ReasonRejectedJob = reason; // Store the rejection reason
            job.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
        public Task<Job> CreateJobAsync(Job job)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteJobAsync(int jobId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Job>> GetAllJobsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Job> GetJobByIdAsync(int jobId)
        {
            throw new NotImplementedException();
        }

        public async Task<Job> GetJobDetail(int id)
        {
            return await _dbSet.Include(j => j.User).Include(t => t.Tags).FirstOrDefaultAsync(c => c.JobID == id);
        }

        public async Task<IEnumerable<Job>> GetJobsByEmployerId(int employerId)
        {
            return await _context.Jobs
                .Where(j => j.UserID == employerId)
                .Include(j => j.User)
                .Include(j => j.Tags)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> HideJobAsync(int jobId)
        {
            var job = await _dbSet.FindAsync(jobId);
            if (job == null) return false;

            job.Status = JobStatus.Hidden;
            job.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Job>> GetJobsByStatus(JobStatus status)
        {
            return await _dbSet.Where(j => j.Status == status).ToListAsync();
        }

        public async Task<(IEnumerable<Job> Jobs, int Total)> GetListSearch(JobSearchRequestDto request)
        {
            DbSet<Job> _JobdbSet = _context.Set<Job>();

            var query = _JobdbSet.Include(c => c.User).AsQueryable();
            query = query.Where(c => c.Status == JobStatus.Active);

            if (!string.IsNullOrWhiteSpace(request.Category) && !request.Category.Equals("All Categories"))
            {
                query = query.Where(c => c.CategoryID.Contains(request.Category)); 
            }    

            if (!string.IsNullOrWhiteSpace(request.JobPosition))
            {
                query = query.Where(c => c.JobPosition.ToLower().Contains(request.JobPosition.ToLower()));
            }

            if (request.WorkTypes != null && request.WorkTypes.Any())
            {
                query = query.Where(c => request.WorkTypes.Select(wt => wt.ToLower()).Contains(c.WorkType.ToLower()));

            }

            if (!string.IsNullOrWhiteSpace(request.Location))
            {
                query = query.Where(c => c.Location.ToLower().Contains(request.Location.ToLower()));
            }

            if (request.Tags != null && request.Tags.Any())
            {
                query = query.Where(c => c.Tags.Any(t => request.Tags.Contains(t.TagID)));
            }

            // Đầu tiên sắp xếp theo Priority (true lên trước)
            var orderedQuery = query.OrderByDescending(c => c.Priority);

            // Sau đó sắp xếp theo UpdatedAt dựa vào MostRecent
            if (request.MostRecent)
            {
                // Nếu MostRecent = true, sắp xếp theo UpdatedAt giảm dần (mới nhất lên đầu)
                query = orderedQuery.ThenByDescending(c => c.UpdatedAt);
            }
            else
            {
                // Nếu MostRecent = false, sắp xếp theo UpdatedAt tăng dần (cũ nhất lên đầu)
                query = orderedQuery.ThenBy(c => c.UpdatedAt);
            }
            // Tải dữ liệu về bộ nhớ trước khi xử lý Salary
            var jobList = await query.ToListAsync();
            var titleList = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                titleList = request.Title.Split(',')
                                         .Select(t => t.Trim().ToLower())
                                         .Where(t => !string.IsNullOrEmpty(t))
                                         .ToList();
            }
            if (titleList.Any())
            {
                jobList = jobList.Where(c =>
                    titleList.Any(keyword =>
                    {
                        var title = c.Title.ToLower();

                        if (keyword.Contains(" "))
                            return title.Contains(keyword);
                        else
                            return Regex.IsMatch(title, $@"\b{Regex.Escape(keyword)}\b");
                    })
                ).ToList();
            }

            if (request.MinSalary.HasValue)
            {
                double minSalary = request.MinSalary.Value;
                jobList = jobList.Where(c => c.Salary != null
                    && c.Salary.Contains("-")
                    && c.Salary.Split('-').Length == 2
                    && double.TryParse(c.Salary.Split('-')[0], out double min)
                    && min >= minSalary).ToList();
            }

            /*if (request.MaxSalary.HasValue)
            {
                double maxSalary = request.MaxSalary.Value;
                jobList = jobList.Where(c => c.Salary != null
                    && c.Salary.Contains("-")
                    && c.Salary.Split('-').Length == 2
                    && double.TryParse(c.Salary.Split('-')[1], out double max)
                    && max <= maxSalary).ToList();
            }*/
            // Lấy tổng số bản ghi trước khi phân trang
            int total = jobList.Count();

            var Jobs = jobList
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return (Jobs, total);
        }

        public async Task<(IEnumerable<Job> Jobs, int Total)> GetJobsForManagement(JobSearchRequestDto request)
        {
            DbSet<Job> _JobdbSet = _context.Set<Job>();
            var query = _JobdbSet.Include(c => c.User).AsQueryable();
            if (request.UserID.HasValue && request.UserID > 0)
            {
                query = query.Where(c => c.UserID == request.UserID);
            }
            // Filter by title
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                query = query.Where(c => c.Title.ToLower().Contains(request.Title.ToLower()));
            }

            // Filter by job position
            if (!string.IsNullOrWhiteSpace(request.JobPosition))
            {
                query = query.Where(c => c.JobPosition.ToLower().Contains(request.JobPosition.ToLower()));
            }

            // Filter by work types
            if (request.WorkTypes != null && request.WorkTypes.Any())
            {
                query = query.Where(c => request.WorkTypes.Select(wt => wt.ToLower()).Contains(c.WorkType.ToLower()));
            }

            // Filter by location
            if (!string.IsNullOrWhiteSpace(request.Location))
            {
                query = query.Where(c => c.Location.ToLower().Contains(request.Location.ToLower()));
            }

            if (request.Tags != null && request.Tags.Any())
            {
                query = query.Where(c => c.Tags.Any(t => request.Tags.Contains(t.TagID)));
            }

            if (!request.MostRecent)
            {
                query = query.OrderByDescending(c => c.UpdatedAt);
            }
           
            // Tải dữ liệu về bộ nhớ trước khi xử lý Salary
            var jobList = await query.ToListAsync();

            if (request.MinSalary.HasValue)
            {
                double minSalary = request.MinSalary.Value;
                jobList = jobList.Where(c => c.Salary != null
                    && c.Salary.Contains("-")
                    && c.Salary.Split('-').Length == 2
                    && double.TryParse(c.Salary.Split('-')[0], out double min)
                    && min >= minSalary).ToList();
            }

            /*if (request.MaxSalary.HasValue)
            {
                double maxSalary = request.MaxSalary.Value;
                jobList = jobList.Where(c => c.Salary != null
                    && c.Salary.Contains("-")
                    && c.Salary.Split('-').Length == 2
                    && double.TryParse(c.Salary.Split('-')[1], out double max)
                    && max <= maxSalary).ToList();
            }*/
            // Lấy tổng số bản ghi trước khi phân trang
            int total = jobList.Count();

            var Jobs = jobList
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return (Jobs, total);
        }
        public async Task<bool> UnhideJobAsync(int jobId)
        {
            var job = await _dbSet.FindAsync(jobId);
            if (job == null) return false;

            // Revert to previous status or set to Active
            job.Status = JobStatus.Active;
            job.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public Task<Job> UpdateJobAsync(Job job)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateJobStatus(int jobId, JobStatus newStatus)
        {
            var job = await _dbSet.FindAsync(jobId);
            if (job == null) return false;

            job.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<Job>> GetExpiredJobsAsync()
        {
            var currentDate = DateTime.Now;
            return await _dbSet
                .Where(j => j.Deadline < currentDate)
                .ToListAsync();
        }

        public async Task<List<Job>> HideExpiredJobsAsync()
        {
            var expiredJobs = await GetExpiredJobsAsync();

            foreach (var job in expiredJobs)
            {
                job.Status = JobStatus.Hidden;
            }

            if (expiredJobs.Any())
            {
                await _context.SaveChangesAsync();
            }

            return expiredJobs;
        }


        public async Task<Job> GetJobDetailForApplicationAsync(int applicationId)
        {
            var application = await _context.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);

            return application?.Job;
        }

        public async Task<List<Job>> GetSimilarJob(int jobId)
        {
            var job = await _dbSet.Include(j => j.Tags).Include(u => u.User)
                      .FirstOrDefaultAsync(j => j.JobID == jobId);

            if (job == null || job.Tags == null || !job.Tags.Any())
                return new List<Job>(); // Trả về danh sách rỗng nếu không tìm thấy job hoặc không có Tags

            var jobTagIds = job.Tags.Select(t => t.TagID).ToList(); // Lấy danh sách TagID của job

            var similarJobs = _dbSet.Include(j => j.Tags)
                .Where(j => j.JobID != jobId)
                .AsEnumerable() // Chuyển truy vấn về bộ nhớ
                .Where(j => j.Tags.Any(t => jobTagIds.Contains(t.TagID))) // Lọc các job có Tag trùng
                .OrderByDescending(j => j.Tags.Count)
                .Take(3)
                .ToList(); // Đổi ToListAsync() thành ToList()

            return similarJobs;
        }
        public async Task<Job> GetByJobId(int id)
        {
            return await _dbSet.Include(t => t.Tags).FirstOrDefaultAsync(j => j.JobID == id);
        }
        // lấy đường dãn tới file cấu hình 
        private async Task<int> GetMaxJobsPerDayFromSettings()
        {
            int defaultLimit = 1;

            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string solutionDirectory = Path.GetFullPath(Path.Combine(currentDirectory, "..\\..\\..\\.."));
                string settingsFilePath = Path.Combine(solutionDirectory, "WorkSmart.API", "freePlanSettings.json");

                if (File.Exists(settingsFilePath))
                {
                    string jsonData = await File.ReadAllTextAsync(settingsFilePath);
                    if (!string.IsNullOrWhiteSpace(jsonData))
                    {
                        var settings = JsonConvert.DeserializeObject<FreePlanSettings>(jsonData);
                        if (settings != null && settings.employerFreePlan != null)
                        {
                            defaultLimit = settings.employerFreePlan.MaxJobsPerDay;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error reading job limit settings: {ex.Message}");
            }

            return defaultLimit;
        }
        public async Task<bool> CheckLimitCreateJob(int userID, int? maxJobsPerDayFromClient = null)
        {
            var today = DateTime.UtcNow.Date;
            var jobCount = await _dbSet.CountAsync(u =>
                u.UserID == userID && EF.Functions.DateDiffDay(u.CreatedAt, today) == 0);

            
            var subscription = await _context.Subscriptions
                                           .Include(p => p.Package)
                                           .FirstOrDefaultAsync(u => u.UserID == userID);

            if (subscription != null)
            {
                return jobCount < subscription.Package.JobPostLimitPerDay;
            }


            int defaultLimit = await GetMaxJobsPerDayFromSettings();


            int limitToUse = maxJobsPerDayFromClient ?? defaultLimit;

            return jobCount < limitToUse;
        }

        public async Task<bool> CheckLimitCreateFeaturedJob(int userID)
        {
            var featuredJobCount = await _dbSet.CountAsync(u =>
                u.UserID == userID && u.Priority == true);

            var subscription = await _context.Subscriptions
                                             .Include(p => p.Package)
                                             .FirstOrDefaultAsync(u => u.UserID == userID);

            if (subscription != null)
            {
                return featuredJobCount < subscription.Package.FeaturedJobPostLimit;
            }

            int defaultFeaturedLimit = await GetDefaultFeaturedJobLimit();

            return featuredJobCount < defaultFeaturedLimit;
        }

        private async Task<int> GetDefaultFeaturedJobLimit()
        {
            int defaultLimit = 0; // Default to 0 instead of 1

            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string solutionDirectory = Path.GetFullPath(Path.Combine(currentDirectory, "..\\..\\..\\.."));
                string settingsFilePath = Path.Combine(solutionDirectory, "WorkSmart.API", "freePlanSettings.json");

                if (File.Exists(settingsFilePath))
                {
                    string jsonData = await File.ReadAllTextAsync(settingsFilePath);
                    if (!string.IsNullOrWhiteSpace(jsonData))
                    {
                        var settings = JsonConvert.DeserializeObject<FreePlanSettings>(jsonData);
                        if (settings != null && settings.employerFreePlan != null)
                        {
                            defaultLimit = settings.employerFreePlan.DefaultFeaturedJob;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần thiết
                Console.WriteLine($"Error reading job limit settings: {ex.Message}");
            }

            return defaultLimit;
        }
        // Set lai độ ưu tiên
        public async Task<bool> ToggleJobPriorityAsync(int jobId)
        {
            var job = await _dbSet.FindAsync(jobId);
            if (job == null) return false;
            //Kiểm tra có đang ưu tiên không
            if (job.Priority)
            {
                
                //var subscription = await _context.Subscriptions
                //    .Include(s => s.Package)
                //    .FirstOrDefaultAsync(s => s.UserID == job.UserID);
                ////kiểm tra gói
                //if (subscription != null)
                //{
                //    // kiểm tra với gói có duration tới today đã 
                //    var subscriptionStartDate = subscription.CreatedAt;
                //    var durationInDays = subscription.Package.DurationInDays;
                //    var expirationDate = subscriptionStartDate.AddDays(durationInDays);
                //    // nếu current day đến ngày hết hạn thì không cho set lại
                //    if (DateTime.Now < expirationDate)
                //    {
                //        return false;
                //    }
                //}
            }
            else
            {
                bool canCreateFeatured = await CheckLimitCreateFeaturedJob(job.UserID);
                if (!canCreateFeatured)
                {
                    return false;
                }
            }

            // Toggle the priority
            job.Priority = !job.Priority;

            // Update the job's UpdatedAt timestamp
            //job.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<Job>> GetExpiringJobsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var threeDaysLater = today.AddDays(3);

            return await _dbSet
                .Where(j => j.Status == JobStatus.Active &&
                           j.Deadline.HasValue &&
                           j.Deadline.Value.Date > today &&
                           j.Deadline.Value.Date <= threeDaysLater)
                .ToListAsync();
        }

        public async Task<List<Job>> GetExpiredJobs()
        {
            var today = DateTime.UtcNow.Date;

            return await _dbSet
                .Where(j => j.Status == JobStatus.Active &&
                           j.Deadline.HasValue &&
                           today.AddDays(-1).Date == j.Deadline.Value.Date)
                .ToListAsync();
        }

        public async Task<bool> UpdateJobStatusAsync(int jobId, JobStatus newStatus)
        {
            var job = await _dbSet.FindAsync(jobId);
            if (job == null) return false;

            job.Status = newStatus;
            job.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<object>> JobCategoryDashboard()
        {
            var totalJobs = await _context.Jobs.CountAsync(); // Tổng số job

            var result = await _context.Jobs
                .GroupBy(j => j.CategoryID)
                .Select(g => new
                {
                    Category = g.Key ?? "Unknown", // Tránh lỗi nếu CategoryID null
                    Percentage = Math.Round((double)g.Count() / totalJobs * 100, 2) // Tính % và làm tròn 2 chữ số
                })
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<object>> JobStatusDashboard()
        {
            var currentYear = DateTime.Now.Year;

            // Lấy danh sách số lượng job được đăng trong tháng
            var jobCountsByMonth = await _context.Jobs
                .Where(j => j.CreatedAt.Year == currentYear)
                .GroupBy(j => j.CreatedAt.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Posted = g.Count()
                })
                .ToListAsync();

            // Lấy danh sách số lượng job hết hạn theo tổng tất cả job trong năm
            var totalExpiredCountsByMonth = await _context.Jobs
                .Where(j => j.Deadline.HasValue && j.Deadline.Value.Year == currentYear)
                .GroupBy(j => j.Deadline.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Expired = g.Count()
                })
                .ToListAsync();

            // Lấy số lượng ứng tuyển được duyệt theo tháng
            var filledCountsByMonth = await _context.Applications
                .Where(a => a.CreatedAt.Year == currentYear && a.Status == "Approved")
                .GroupBy(a => a.CreatedAt.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Filled = g.Count()
                })
                .ToListAsync();

            // Danh sách mặc định 12 tháng
            var months = Enumerable.Range(1, 12).Select(m => new
            {
                Month = m,
                Posted = 0,
                Filled = 0,
                Expired = 0
            }).ToList();

            // Ghép dữ liệu Posted
            foreach (var item in jobCountsByMonth)
            {
                var index = item.Month - 1;
                months[index] = new
                {
                    Month = item.Month,
                    Posted = item.Posted,
                    Filled = months[index].Filled,
                    Expired = months[index].Expired
                };
            }

            // Ghép dữ liệu Expired
            foreach (var item in totalExpiredCountsByMonth)
            {
                var index = item.Month - 1;
                months[index] = new
                {
                    Month = months[index].Month,
                    Posted = months[index].Posted,
                    Filled = months[index].Filled,
                    Expired = item.Expired
                };
            }

            // Ghép dữ liệu Filled
            foreach (var item in filledCountsByMonth)
            {
                var index = item.Month - 1;
                months[index] = new
                {
                    Month = months[index].Month,
                    Posted = months[index].Posted,
                    Filled = item.Filled,
                    Expired = months[index].Expired
                };
            }

            // Chuyển đổi số tháng thành tên tháng
            var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            var result = months.Select(m => new
            {
                Month = monthNames[m.Month - 1],
                Posted = m.Posted,
                Filled = m.Filled,
                Expired = m.Expired
            }).ToList();

            return result;
        }
        public async Task<IEnumerable<object>> JobLocationDashboard()
        {
            var jobs = await _context.Jobs
                .Where(j => !string.IsNullOrEmpty(j.Location))
                .Select(j => j.Location)
                .ToListAsync();

            // Tách các thành phố từ Location
            var locationCounts = jobs
                .SelectMany(loc => loc.Split(',').Select(city => city.Trim()))
                .GroupBy(city => city)
                .Select(g => new
                {
                    Name = g.Key,
                    Value = g.Count()
                })
                .OrderByDescending(l => l.Value) // Sắp xếp giảm dần
                .ToList();

            // Lấy tổng số công việc
            int totalJobs = locationCounts.Sum(l => l.Value);

            // Chỉ lấy 4 thành phố có nhiều job nhất
            var topLocations = locationCounts.Take(4).ToList();

            // Tính tổng số lượng các thành phố còn lại
            var otherCount = locationCounts.Skip(4).Sum(l => l.Value);

            // Chuyển đổi sang phần trăm
            var locationPercentages = topLocations
                .Select(l => new
                {
                    Name = l.Name,
                    Value = Math.Round((double)l.Value / totalJobs * 100, 2) // Tính % & làm tròn 2 chữ số
                })
                .ToList();

            // Nếu có thành phố khác ngoài top 4, thêm vào "Others"
            if (otherCount > 0)
            {
                locationPercentages.Add(new
                {
                    Name = "Others",
                    Value = Math.Round((double)otherCount / totalJobs * 100, 2)
                });
            }

            return locationPercentages;
        }

        public async Task<IEnumerable<int>> GetJobIdsByUserIdAsync(int userId)
        {
            return await _context.Jobs
                .Where(j => j.UserID == userId)
                .Select(j => j.JobID)
                .ToListAsync();
        }
        public async Task<List<Job>> GetAllJobActive()
        {
            return await _dbSet.Include(j => j.User)
                .Where(j => j.Status == JobStatus.Active)
                .ToListAsync();
        }
    }
}
