﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Helpers;

namespace WorkSmart.Core.Dto.TransactionDtos
{
    public class GetListTransactionDto
    {
        public int TransactionID { get; set; }
        public int UserID { get; set; }
        public long OrderCode { get; set; }
        public string Content { get; set; }
        public double Price { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = TimeHelper.GetVietnamTime();
        public DateTime UpdatedAt { get; set; } = TimeHelper.GetVietnamTime();

        public string UserName { get; set; }
        public string UserEmail { get; set; }
    }
}
