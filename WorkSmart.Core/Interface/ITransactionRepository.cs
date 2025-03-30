using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.TransactionDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface ITransactionRepository : IBaseRepository<Transaction>
    {
        Task<List<Transaction>> GetAllByUserId(int userId);
    }
}
