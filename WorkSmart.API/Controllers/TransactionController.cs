using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.AdminDtos;
using WorkSmart.Core.Dto.TransactionDtos;
using WorkSmart.Core.Interface;
using WorkSmart.Repository.Repository;

namespace WorkSmart.API.Controllers
{
    [Route("transactions")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;
        private readonly TransactionService _transactionService;

        public TransactionController(IAccountRepository accountRepository, IMapper mapper, TransactionService transactionService)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;
            _transactionService = transactionService;
        }

        [HttpGet("getByUserId/{id}")]
        public async Task<IActionResult> GetByUserId(int id)
        {
            try
            {
                var transactions = await _transactionService.GetAllByUserId(id);
                if (transactions == null || !transactions.Any())
                    return NotFound(new { message = "No transactions found" });

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
