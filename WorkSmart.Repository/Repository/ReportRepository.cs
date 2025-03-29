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
    public class ReportRepository : BaseRepository<ReportPost>, IReportRepository
    {
        private readonly WorksmartDBContext _context;

        public ReportRepository(WorksmartDBContext context) : base(context) 
        {
            _context = context;
        }

        public async Task<ReportPost> CreateReport(ReportPost report)
        {
            await _context.ReportPosts.AddAsync(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<IEnumerable<ReportPost>> GetReportsByAdmin()
        {
            var reports = await _context.ReportPosts
                .Include(r => r.Sender)
                .Include(r => r.Job)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return reports;
        }
        public async Task<ReportPost> CheckReportStatus(int userId, int jobId)
        {
            return await _context.ReportPosts
                .Where(r => r.SenderID == userId && r.JobID == jobId)
                .FirstOrDefaultAsync();
        }
    }
}
