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
                .Include(c => c.Experiences)
                .Include(c => c.Educations)
                .Include(c => c.Certifications)
                .Include(c => c.Skills)
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
    }
}