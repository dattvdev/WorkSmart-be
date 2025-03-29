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
    public class SubscriptionRepository : BaseRepository<Subscription>, ISubscriptionRepository
    {
        private readonly WorksmartDBContext _context;
        public SubscriptionRepository(WorksmartDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Subscription> GetByUserId(int id)
        {
           return await _dbSet.Where(x => x.UserID == id).FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<object>> SubscriptionRevenueDashboard()
        {
            var currentYear = DateTime.Now.Year;

            // Lấy danh sách tất cả gói package hiện có
            var packages = await _context.Packages
                .Select(p => p.Name)
                .ToListAsync();

            // Lấy dữ liệu Subscription trong năm hiện tại và nhóm theo tháng + gói
            var revenueData = await _context.Subscriptions
                .Include(s => s.Package)
                .Where(s => s.CreatedAt.Year == currentYear)
                .GroupBy(s => new { s.CreatedAt.Month, PackageName = s.Package.Name })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    PackageName = g.Key.PackageName, // Sửa lại tên đúng
                    Revenue = g.Sum(s => s.Package.Price) // Tính tổng doanh thu theo gói
                })
                .ToListAsync();


            // Danh sách mặc định 12 tháng, mỗi tháng chứa tất cả gói package hiện có
            var months = Enumerable.Range(1, 12).Select(m => new Dictionary<string, object>
                {
                    { "Month", m },
                    { "Total", 0.0 }
                }).ToList();

            // Thêm tất cả package vào từng tháng
            foreach (var month in months)
            {
                foreach (var package in packages)
                {
                    month[package] = 0.0;
                }
            }

            // Gán dữ liệu từ database vào danh sách tháng
            foreach (var item in revenueData)
            {
                var index = item.Month - 1;
                var updated = months[index];

                updated[item.PackageName] = (double)updated[item.PackageName] + item.Revenue;
                updated["Total"] = (double)updated["Total"] + item.Revenue;
            }

            // Chuyển đổi số tháng thành tên tháng
            var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            var result = months.Select(m =>
            {
                var monthObj = new Dictionary<string, object>
            {
                { "Month", monthNames[(int)m["Month"] - 1] },
                { "Total", m["Total"] }
            };

                foreach (var package in packages)
                {
                    monthObj[package] = m[package];
                }

                return monthObj;
            }).ToList();

            return result;
        }

    }
}
