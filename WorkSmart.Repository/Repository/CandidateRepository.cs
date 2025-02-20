using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.CandidateDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class CandidateRepository : BaseRepository<User>, ICandidateRepository
    {
        public CandidateRepository(WorksmartDBContext context) : base(context)
        {
        }

        public async Task<IEnumerable<User>> GetListSearch(CandidateSearchRequestDto request)
        {
            DbSet<CV> _CVdbSet = _context.Set<CV>();
            var query = _CVdbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                query = query.Where(c => c.FullName.ToLower().Contains(request.Name.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(request.JobPosition) && request.Exp > 0)
            query = query.Where(c => c.Experiences.Any(e =>
                ((e.EndedAt ?? DateTime.Now).Year - e.StartedAt.Year) >= request.Exp &&
                e.JobPosition.Equals(request.JobPosition, StringComparison.OrdinalIgnoreCase)
                ));

            if (!string.IsNullOrWhiteSpace(request.Education))
            {
                query = query.Where(c => c.Educations.Any(e => 
                                e.Degree.Equals(request.Education, StringComparison.OrdinalIgnoreCase)));
            }
            if (!string.IsNullOrWhiteSpace(request.Major))
            {
                query = query.Where(c => c.Educations.Any(e => 
                                e.Major.Equals(request.Major, StringComparison.OrdinalIgnoreCase)));
            }
            if (!string.IsNullOrWhiteSpace(request.WorkType))
            {
                query = query.Where(c => c.WorkType.Equals(request.WorkType, StringComparison.OrdinalIgnoreCase));
            }
            // Lấy danh sách UserId không trùng
            var userIds = await query
                .Select(c => c.UserID)
                .Distinct()
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            if (!userIds.Any())
            {
                return new List<User>();
            }
            // Truy vấn danh sách User dựa trên UserId
            var users = await _context.Users
                .Where(u => userIds.Contains(u.UserID))
                .ToListAsync();
            return users;
        }
    }
}
