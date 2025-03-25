using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using System.ComponentModel.Design;
using SystemTransaction = System.Transactions.Transaction;
using WorkSmart.Core.Dto.PaymentDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;
using WorkSmart.Repository;
using Microsoft.EntityFrameworkCore;

namespace WorkSmart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PayOS _payOS;
        private readonly WorksmartDBContext _context;
        private readonly IAccountRepository _accountRepository;
        private readonly WorkSmart.Core.Entity.Transaction transaction;

        public PaymentController(PayOS payOS, WorksmartDBContext context, IAccountRepository accountRepository)
        {
            _payOS = payOS;
            _context = context;
            _accountRepository = accountRepository;
        }

        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePaymentLink([FromBody] PaymentRequestDto request)
        {
            try
            {
                var package = await _context.Packages
                    .FirstOrDefaultAsync(p => p.PackageID == request.PackageId);

                if (package == null)
                {
                    return NotFound("Package not exist");
                }

                long orderCode = long.Parse(DateTimeOffset.Now.ToString("yyyyMMddHHmmss"));

                var transaction = new WorkSmart.Core.Entity.Transaction
                {
                    UserID = request.UserId,
                    OrderCode = orderCode,
                    Content = $"Pay {package.Name}",
                    Price = package.Price,
                    Status = "PENDING",
                    CreatedAt = DateTime.Now
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                var items = new List<ItemData>
                {
                    new ItemData(
                        package.Name,
                        1,
                        Convert.ToInt32(package.Price)
                    )
                };

                var paymentData = new PaymentData(
                    orderCode,
                    (int)package.Price,
                    $"Pay {package.Name}",
                    items,
                    "http://localhost:5173/payment/cancel",
                    "http://localhost:5173/payment/success"
                );

                var createPayment = await _payOS.createPaymentLink(paymentData);

                return Ok(new PaymentResponseDto
                {
                    CheckoutUrl = createPayment.checkoutUrl,
                    OrderCode = orderCode,
                    Amount = package.Price,
                    PackageName = package.Name
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while create payment", Details = ex.Message });

            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandlePaymentWebhook(WebhookType body)
        {
            try
            {
                WebhookData data = _payOS.verifyPaymentWebhookData(body);

                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.OrderCode == data.orderCode);

                if (transaction == null)
                {
                    return NotFound("Payment not found");
                }

                var package = await _context.Packages
                    .FirstOrDefaultAsync(p => p.Name == transaction.Content.Replace("Pay ", ""));

                if (package == null)
                {
                    return NotFound("Package not found");
                }

                if (body.success)
                {
                    transaction.Status = "SUCCESS";

                    var existingSubscription = await _context.Subscriptions
                        .FirstOrDefaultAsync(s => s.UserID == transaction.UserID && s.PackageID == package.PackageID);

                    if (existingSubscription == null)
                    {
                        var subscription = new Subscription
                        {
                            UserID = transaction.UserID,
                            PackageID = package.PackageID,
                            ExpDate = DateTime.Now.AddDays(30), // Mặc định 30 ngày
                            CreatedAt = DateTime.Now
                        };

                        _context.Subscriptions.Add(subscription);
                    }
                }
                else
                {
                    transaction.Status = "FAILED";
                }

                await _context.SaveChangesAsync();

                return Ok("Webhook processed successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while handle webhook", Details = ex.Message });

            }
        }

        [HttpGet("payment-status/{orderCode}")]
        public async Task<IActionResult> CheckPaymentStatus(long orderCode)
        {
            try 
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.OrderCode == orderCode);

                if (transaction == null)
                {
                    return NotFound("Payment not found");
                }

                return Ok(new PaymentStatusDto
                {
                    OrderCode = transaction.OrderCode,
                    Status = transaction.Status,
                    Price = transaction.Price,
                    Content = transaction.Content
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while payment", Details = ex.Message });

            }
        }
    }
}
