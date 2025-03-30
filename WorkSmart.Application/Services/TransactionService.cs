using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.PackageDtos;
using WorkSmart.Core.Dto.SubscriptionDtos;
using WorkSmart.Core.Dto.TransactionDtos;
using WorkSmart.Core.Interface;
using WorkSmart.Repository.Repository;

namespace WorkSmart.Application.Services
{
    public class TransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;

        public TransactionService(ITransactionRepository transactionRepository, IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _mapper = mapper;
        }

        public async Task<List<GetListTransactionDto>> GetAllByUserId(int userId)
        {
            var transactions = await _transactionRepository.GetAllByUserId(userId);
            return _mapper.Map<List<GetListTransactionDto>>(transactions);
        }
    }
}
