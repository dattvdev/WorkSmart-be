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
    public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(WorksmartDBContext context) : base(context)
        {
        }

        public async Task<List<Transaction>> GetAllByUserId(int userId)
        {
            return await _context.Transactions
                .Include(t => t.User)
                .Where(t => t.UserID == userId && (t.Status == "PAID" || t.Status == "FAILED"))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}
