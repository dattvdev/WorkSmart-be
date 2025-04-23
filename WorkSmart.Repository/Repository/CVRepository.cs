using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WorkSmart.Core.Dto.CVDtos;
using WorkSmart.Core.Dto.JobDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Enums;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class CVRepository : BaseRepository<CV>, ICVRepository
    {
        public CVRepository(WorksmartDBContext context) : base(context) { }

        public async Task<IEnumerable<CV>> GetAllCVsByUserId(int userId)
        {
            return await _dbSet
                .Include(c => c.Experiences)
                .Include(c => c.Educations)
                .Include(c => c.Certifications)
                .Include(c => c.Skills)
                .Where(cv => cv.UserID == userId && cv.IsHidden == false)
                .OrderByDescending(cv => cv.CreatedAt)
                .ToListAsync();
        }

        public async Task<CV> GetCVWithDetails(int id)
        {
            return await _dbSet.Include(c => c.Experiences)
                               .Include(c => c.Certifications)
                               .Include(c => c.Skills)
                               .Include(c => c.Educations)
                               .FirstOrDefaultAsync(c => c.CVID == id);
        }
        public void Update(CV cv)
        {
            _context.Entry(cv).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public async Task Delete(int id)
        {
            var cv = await GetById(id);
            if (cv != null)
            {
                _dbSet.Remove(cv);
                await _context.SaveChangesAsync(); 
            }
        }

        public void SetFeature(int cvId, int userId)
        {
            var cvs = _dbSet.Where(cv => cv.UserID == userId).ToList();
            foreach (var cv in cvs)
            {
                if (cv.CVID == cvId)
                {
                    if(cv.IsFeatured == null) cv.IsFeatured = true;
                    else
                        cv.IsFeatured = !cv.IsFeatured;
                }
                else cv.IsFeatured = false;
            }
            _context.SaveChanges();
        }

        public void HideCV(int cvId)
        {
            var cv = _dbSet.FirstOrDefault(cv => cv.CVID == cvId);
            if (cv != null)
            {
                cv.IsHidden = true;
                _dbSet.Update(cv);
                _context.SaveChanges();
            }
        }

        public async Task<CVCreationLimitDto> GetRemainingCVCreationLimit(int userID)
        {
            var today = DateTime.UtcNow.Date;

            var totalSystemCreatedCVs = await _dbSet.CountAsync(cv => 
                cv.UserID == userID &&
                string.IsNullOrEmpty(cv.FileName) &&
                string.IsNullOrEmpty(cv.FilePath) &&
                EF.Functions.DateDiffDay(cv.CreatedAt, today) == 0 &&
                (!cv.IsHidden.HasValue || cv.IsHidden == false));

            var user = await _context.Users.FindAsync(userID);
            if (user == null)
            {
                throw new Exception($"User with ID {userID} not found");
            }

            var activeSubscriptions = await _context.Subscriptions
                .Include(s => s.Package)
                .Where(s => s.UserID == userID && s.ExpDate > DateTime.Now)
                .ToListAsync();

            int cvLimit;
            string packageName = "Free";

            if (activeSubscriptions.Any())
            {
                var packagePriority = new Dictionary<string, int>();

                if (user.Role == "Candidate")
                {
                    packagePriority = new Dictionary<string, int>
            {
                { "Candidate Pro", 3 },
                { "Candidate Plus", 2 },
                { "Candidate Basic", 1 }
            };
                }

                var highestSubscription = activeSubscriptions
                    .OrderByDescending(s => packagePriority.ContainsKey(s.Package.Name)
                        ? packagePriority[s.Package.Name]
                        : 0)
                    .First();

                if (highestSubscription.Package.CVLimit.HasValue)
                {
                    cvLimit = highestSubscription.Package.CVLimit.Value;
                    packageName = highestSubscription.Package.Name;
                }
                else
                {
                    cvLimit = await GetMaxCVsFromSettings();
                }
            }
            else
            {
                cvLimit = await GetMaxCVsFromSettings();
            }

            int remainingLimit = Math.Max(0, cvLimit - totalSystemCreatedCVs);

            return new CVCreationLimitDto
            {
                RemainingLimit = remainingLimit,
                TotalLimit = cvLimit,
                UsedTotal = totalSystemCreatedCVs,
                Message = $"User has {remainingLimit} CV creation attempts left. ({totalSystemCreatedCVs}/{cvLimit} used)",
                PackageName = packageName
            };
        }

        private async Task<int> GetMaxCVsFromSettings()
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
                        if (settings != null && settings.candidateFreePlan != null)
                        {
                            defaultLimit = settings.candidateFreePlan.MaxCVsPerDay;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CV limit settings: {ex.Message}");
            }
            return defaultLimit;
        }
    }
}