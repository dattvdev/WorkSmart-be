using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class ReportRepository : IReportRepository
    {
        private readonly WorksmartDBContext _context;

        public ReportRepository(WorksmartDBContext context)
        {
            _context = context;
        }

        public async Task<ReportPost> CreateReport(ReportPost report)
        {
            await _context.ReportPosts.AddAsync(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<(IEnumerable<ReportPost> Reports, int Total)> GetReportsByAdmin(int pageNumber, int pageSize)
        {
            var reports = await _context.ReportPosts
                .Include(r => r.Sender)
                .Include(r => r.Job)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int total = await _context.ReportPosts.CountAsync();

            return (reports, total);
        }


        public async Task<int> GetTotalReportCount()
        {
            return await _context.ReportPosts.CountAsync();
        }
    }
}
