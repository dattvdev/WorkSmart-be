using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Dto.PaymentDtos
{
    public class TransactionDto
    {
        public long OrderCode { get; set; }
        public double Price { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }
        // Chỉ chứa các thuộc tính cần thiết
    }
}
