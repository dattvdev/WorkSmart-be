using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.TransactionDtos;
using WorkSmart.Core.Dto.UserDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Application.Mapper
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<Transaction, GetListTransactionDto>().ReverseMap();
        }
    }
}
