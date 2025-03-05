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

        public async Task<(IEnumerable<User> Users, int Total)> GetListSearch(CandidateSearchRequestDto request)
        {
            DbSet<CV> _CVdbSet = _context.Set<CV>();
            var query = _CVdbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                query = query.Where(c => c.FirstName.ToLower().Contains(request.Name.ToLower()) || c.LastName.ToLower().Contains(request.Name.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(request.JobPosition) && request.Exp > 0)
            {
                query = query.Where(c => c.Experiences.Any(e =>
                    ((e.EndedAt ?? DateTime.Now).Year - e.StartedAt.Value.Year) >= request.Exp &&
                    e.JobPosition.ToLower() == request.JobPosition.ToLower()
                ));
            }

            if (!string.IsNullOrWhiteSpace(request.Education))
            {
                query = query.Where(c => c.Educations.Any(e =>
                    e.Degree.ToLower() == request.Education.ToLower()
                ));
            }

            if (!string.IsNullOrWhiteSpace(request.Major))
            {
                query = query.Where(c => c.Educations.Any(e =>
                    e.Major.ToLower() == request.Major.ToLower()
                ));
            }

            if (!string.IsNullOrWhiteSpace(request.WorkType))
            {
                query = query.Where(c => c.WorkType.ToLower() == request.WorkType.ToLower());
            }
            query = query.Include(c => c.User);

            var cvs = await query.ToListAsync();
            query = query.Where(c => !c.User.IsPrivated);
            // Lấy tổng số bản ghi trước khi phân trang
            int total = await query.Select(c => c.UserID).Distinct().CountAsync();

            // Lấy danh sách UserId không trùng
            var userIds = await query
                .Select(c => c.UserID) // Lấy UserID
                .Distinct()
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();
            

            if (!userIds.Any())
            {
                return (new List<User>(), total);
            }

            // Truy vấn danh sách User dựa trên UserId
            var users = await _context.Users
                .Where(u => userIds.Contains(u.UserID))
                .ToListAsync();

            return (users, total);
        }

    }
}
