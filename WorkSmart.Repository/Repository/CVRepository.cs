using Microsoft.EntityFrameworkCore;
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
.Where(cv => cv.UserID == userId)
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
    }
}