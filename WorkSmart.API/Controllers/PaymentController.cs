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
                    "http://localhost:5173/employer/payment-cancel",
                    "http://localhost:5173/employer/payment-return",
                    expiredAt: DateTimeOffset.Now.AddMinutes(15).ToUnixTimeSeconds()
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
        public async Task<IActionResult> HandlePaymentWebhook([FromBody] WebhookType body)
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

                bool paymentSuccess = body.success &&
                               data.amount > 0 &&
                               (data.code == "00" || data.desc.ToLower() == "success");

                if (paymentSuccess)
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

                if (transaction.Status == "PAID")
                {
                    return Ok(new
                    {
                        status = "SUCCESS",
                        message = "Thanh toán thành công",
                        details = transaction
                    });
                }

                // Các trạng thái khác
                return Ok(new
                {
                    status = transaction.Status,
                    message = "Trạng thái thanh toán chưa hoàn tất"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while payment", Details = ex.Message });

            }
        }

        [HttpGet("payment-return")]
        public async Task<IActionResult> ProcessPaymentReturn([FromQuery] string code, [FromQuery] string id, [FromQuery] bool cancel, [FromQuery] string status, [FromQuery] long orderCode)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (code != "00" || status != "PAID")
                {
                    return BadRequest("Invalid payment status");
                }

                // Tìm transaction dựa trên orderCode
                var existingTransaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.OrderCode == orderCode);

                if (existingTransaction == null)
                {
                    return NotFound("Transaction not found");
                }

                // Trích xuất thông tin Package từ nội dung giao dịch
                var packageName = existingTransaction.Content.Replace("Pay ", "").Trim();

                var package = await _context.Packages
                    .FirstOrDefaultAsync(p => p.Name == packageName);

                if (package == null)
                {
                    return NotFound($"Package not found for name: {packageName}");
                }

                var existingSubscription = await _context.Subscriptions
                    .FirstOrDefaultAsync(s =>
                        s.UserID == existingTransaction.UserID &&
                        s.PackageID == package.PackageID);

                if (existingSubscription != null)
                {
                    return Ok(new
                    {
                        status = "SUCCESS",
                        message = "Subscription already exists",
                        orderCode = orderCode
                    });
                }

                var newSubscription = new Subscription
                {
                    PackageID = package.PackageID,
                    UserID = existingTransaction.UserID,
                    ExpDate = DateTime.Now.AddDays(package.DurationInDays),
                    CreatedAt = DateTime.Now
                };

                // Cập nhật trạng thái thanh toán
                existingTransaction.Status = "PAID";
                existingTransaction.UpdatedAt = DateTime.Now;

                _context.Subscriptions.Add(newSubscription);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                // Thực hiện các bước tiếp theo sau khi thanh toán thành công 
                // (ví dụ: kích hoạt gói, cập nhật quyền hạn, v.v.)

                return Ok(new
                {
                    status = "SUCCESS",
                    message = "Thanh toán và tạo subscription thành công",
                    orderCode = orderCode,
                    subscriptionId = newSubscription.SubscriptionID,
                    packageName = package.Name
                }); ;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    Error = "An error occurred while processing payment",
                    Details = ex.Message
                });
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelPayment([FromBody] CancelPaymentDto cancelRequest)
        {
            try
            {
                // Tìm giao dịch trong database
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.OrderCode == cancelRequest.OrderCode);

                if (transaction == null)
                {
                    return NotFound(new
                    {
                        StatusCode = 404,
                        Message = "Giao dịch không tồn tại"
                    });
                }

                // Kiểm tra trạng thái hiện tại của giao dịch
                if (transaction.Status == "CANCELLED")
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "Giao dịch đã được hủy trước đó"
                    });
                }

                // Hủy giao dịch trên PayOS
                try
                {
                    var cancelResult = await _payOS.cancelPaymentLink(transaction.OrderCode);

                    // Cập nhật trạng thái giao dịch trong database
                    transaction.Status = "CANCELLED";
                    transaction.CreatedAt = DateTime.Now;
                    transaction.Status = cancelRequest.Reason ?? "User cancelled";

                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        StatusCode = 200,
                        Message = "Hủy thanh toán thành công",
                        OrderCode = transaction.OrderCode
                    });
                }
                catch (Exception payOsEx)
                {

                    return StatusCode(500, new
                    {
                        StatusCode = 500,
                        Message = "Lỗi trong quá trình hủy thanh toán",
                        Details = payOsEx.Message
                    });
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "Đã xảy ra lỗi trong quá trình xử lý",
                    Details = ex.Message
                });
            }
        }
    }
}
