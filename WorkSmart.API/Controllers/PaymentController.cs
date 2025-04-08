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
using WorkSmart.Application.Services;
using WorkSmart.API.SignalRService;

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
        private readonly SignalRNotificationService _signalRService;
        private readonly SendMailService _sendMailService;

        public PaymentController(PayOS payOS, WorksmartDBContext context, IAccountRepository accountRepository, SignalRNotificationService signalRService, SendMailService sendMailService)
        {
            _payOS = payOS;
            _context = context;
            _accountRepository = accountRepository;
            _signalRService = signalRService;
            _sendMailService = sendMailService;
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

                string cancelUrl = request.Role.ToLower() == "candidate"
                    ? "http://localhost:5173/candidate/payment-cancel"
                    : "http://localhost:5173/employer/payment-cancel";

                string returnUrl = request.Role.ToLower() == "candidate"
                    ? "http://localhost:5173/candidate/payment-return"
                    : "http://localhost:5173/employer/payment-return";

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
                    cancelUrl,
                    returnUrl,
                    expiredAt: DateTimeOffset.Now.AddMinutes(5).ToUnixTimeSeconds()
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

        [HttpGet("payment-status/{orderCode}")]
        public async Task<IActionResult> CheckPaymentStatus(long orderCode)
        {
            try
            {
                var transaction = await _context.Transactions
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.OrderCode == orderCode);

                if (transaction == null)
                {
                    return NotFound("Payment not found");
                }

                var transactionDto = new TransactionDto
                {
                    OrderCode = transaction.OrderCode,
                    Price = transaction.Price,
                    Status = transaction.Status,
                    CreatedAt = transaction.CreatedAt,
                    UserFullName = transaction.User.FullName,
                    UserEmail = transaction.User.Email
                };

                return Ok(new
                {
                    status = transaction.Status == "PAID" ? "SUCCESS" : transaction.Status,
                    message = transaction.Status == "PAID" ? "Payment Success" : "Payment pending or failed",
                    details = transactionDto
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
            try
            {
                // Tìm transaction dựa trên orderCode
                var existingTransaction = await _context.Transactions
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.OrderCode == orderCode);

                if (existingTransaction == null)
                {
                    return NotFound("Transaction not found");
                }

                if (existingTransaction.Status == "PAID" ||
                    existingTransaction.Status == "FAILED" ||
                    existingTransaction.Status == "CANCELLED")
                {
                    return Ok(new
                    {
                        status = existingTransaction.Status,
                        message = $"Payment was already processed with status: {existingTransaction.Status}",
                        orderCode = orderCode
                    });
                }

                // Kiểm tra nếu thanh toán bị hủy
                if (cancel || status == "CANCELLED" || status == "FAILED")
                {
                    // Cập nhật trạng thái thanh toán
                    existingTransaction.Status = status == "FAILED" ? "FAILED" : "CANCELLED";
                    existingTransaction.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();

                    string notificationTitle = status == "FAILED" ? "Payment Failed" : "Payment Cancelled";
                    string notificationMessage = status == "FAILED"
                        ? $"Your payment for order {existingTransaction.Content} has failed."
                        : $"Your payment for order {existingTransaction.Content} has been cancelled.";

                    await _signalRService.SendNotificationToUser(
                        existingTransaction.UserID,
                        notificationTitle,
                        notificationMessage,
                         $"employer/transaction-history"
                    );

                    var failureEmailContent = new Core.Dto.MailDtos.MailContent
                    {
                        To = existingTransaction.User.Email,
                        Subject = status == "FAILED" ? "Payment Failed - Transaction Unsuccessful" : "Payment Cancelled - Transaction Terminated",
                        Body = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{(status == "FAILED" ? "Payment Failed" : "Payment Cancelled")}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            margin: 0;
            padding: 0;
            background-color: #f9f9f9;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            background-color: {(status == "FAILED" ? "#e74c3c" : "#f39c12")};
            color: white;
            padding: 20px;
            text-align: center;
        }}
        .content {{
            padding: 30px;
        }}
        .message {{
            background-color: {(status == "FAILED" ? "#fae9e8" : "#fef4e6")};
            border-left: 4px solid {(status == "FAILED" ? "#e74c3c" : "#f39c12")};
            padding: 15px;
            margin-bottom: 20px;
            border-radius: 4px;
        }}
        .transaction-details {{
            background-color: #f5f5f5;
            padding: 15px;
            border-radius: 4px;
            margin-bottom: 20px;
        }}
        .footer {{
            background-color: #f5f5f5;
            padding: 20px;
            text-align: center;
            font-size: 12px;
            color: #777;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h2>{(status == "FAILED" ? "Payment Failed" : "Payment Cancelled")}</h2>
        </div>
        <div class=""content"">
            <h1 style=""color: {(status == "FAILED" ? "#e74c3c" : "#f39c12")}; text-align: center;"">{(status == "FAILED" ? "Transaction Unsuccessful" : "Transaction Terminated")}</h1>
            <div class=""message"">
                <p>Dear {existingTransaction.User.FullName},</p>
                <p>{(status == "FAILED"
                    ? "We regret to inform you that your payment could not be processed successfully. This might be due to insufficient funds, expired card, or other payment issues."
                    : "Your payment has been cancelled as requested or due to session timeout.")}
                </p>
            </div>
            <div class=""transaction-details"">
                <h3>Transaction Details:</h3>
                <p><strong>Order Code:</strong> {existingTransaction.OrderCode}</p>
                <p><strong>Amount:</strong> {existingTransaction.Price:N0} VND</p>
                <p><strong>Date:</strong> {existingTransaction.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss")}</p>
                <p><strong>Status:</strong> {(status == "FAILED" ? "Failed" : "Cancelled")}</p>
            </div>
            <p>
                {(status == "FAILED"
                ? "You can try the payment again or contact your bank for more information about the failed transaction."
                : "You can restart the payment process anytime from your account.")}
            </p>
        </div>
        <div class=""footer"">
            <p>© 2025 WorkSmart. All rights reserved.</p>
            <p>Questions? Contact our support team.</p>
        </div>
    </div>
</body>
</html>"
                    };

                    await _sendMailService.SendMail(failureEmailContent);

                    return Ok(new
                    {
                        status = status == "FAILED" ? "FAILED" : "CANCELLED",
                        message = status == "FAILED" ? "Payment has failed" : "Payment has been canceled",
                        orderCode = orderCode
                    });
                }

                // Xử lý thanh toán thành công
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    if (code != "00" || status != "PAID")
                    {
                        return BadRequest("Invalid payment status");
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
                        .FirstOrDefaultAsync(s => s.UserID == existingTransaction.UserID &&
                                         s.PackageID == package.PackageID);

                    DateTime newExpDate;

                    if (existingSubscription != null)
                    {
                        // Cập nhật subscription hiện có - cộng dồn thời hạn
                        if (existingSubscription.ExpDate > DateTime.Now)
                        {
                            // Nếu subscription vẫn còn hạn, cộng thêm vào thời hạn hiện tại
                            newExpDate = existingSubscription.ExpDate.AddDays(package.DurationInDays);
                        }
                        else
                        {
                            // Nếu subscription đã hết hạn, tính từ hiện tại
                            newExpDate = DateTime.Now.AddDays(package.DurationInDays);
                        }

                        existingSubscription.ExpDate = newExpDate;
                        _context.Subscriptions.Update(existingSubscription);
                    }
                    else
                    {
                        // Tạo subscription mới
                        var newSubscription = new Subscription
                        {
                            PackageID = package.PackageID,
                            UserID = existingTransaction.UserID,
                            ExpDate = DateTime.Now.AddDays(package.DurationInDays),
                            CreatedAt = DateTime.Now
                        };

                        _context.Subscriptions.Add(newSubscription);
                    }

                    existingTransaction.Status = "PAID";
                    existingTransaction.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();

                    await _signalRService.SendNotificationToUser(
                        existingTransaction.UserID,
                        "Payment Successful",
                        $"Your payment for order {"existingTransaction.Content"} has been confirmed.",
                        $"employer/transaction-history"
                    );

                    var successEmailContent = new Core.Dto.MailDtos.MailContent
                    {
                        To = existingTransaction.User.Email,
                        Subject = "Payment Successful - Transaction Confirmed",
                        Body = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Payment Successful</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            margin: 0;
            padding: 0;
            background-color: #f9f9f9;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            background-color: #4CAF50;
            color: white;
            padding: 20px;
            text-align: center;
        }}
        .content {{
            padding: 30px;
        }}
        .message {{
            background-color: #e6f7e6;
            border-left: 4px solid #4CAF50;
            padding: 15px;
            margin-bottom: 20px;
            border-radius: 4px;
        }}
        .transaction-details {{
            background-color: #f5f5f5;
            padding: 15px;
            border-radius: 4px;
            margin-bottom: 20px;
        }}
        .footer {{
            background-color: #f5f5f5;
            padding: 20px;
            text-align: center;
            font-size: 12px;
            color: #777;
        }}
    </style>
</head>
<body>
    <div class=""email-container"">
        <div class=""header"">
            <h2>Payment Successful</h2>
        </div>
        <div class=""content"">
            <h1 style=""color: #4CAF50; text-align: center;"">Transaction Confirmed</h1>
            <div class=""message"">
                <p>Dear {existingTransaction.User.FullName},</p>
                <p>Your payment has been successfully processed. Thank you for your transaction!</p>
            </div>
            <div class=""transaction-details"">
                <h3>Transaction Details:</h3>
                <p><strong>Order Code:</strong> {existingTransaction.OrderCode}</p>
                <p><strong>Amount:</strong> {existingTransaction.Price:N0} VND</p>
                <p><strong>Date:</strong> {existingTransaction.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss")}</p>
                <p><strong>Status:</strong> Paid</p>
                <p><strong>Package:</strong> {packageName}</p>
                <p><strong>Subscription Period:</strong> {(existingSubscription != null && existingSubscription.ExpDate > DateTime.Now ? "Extended until" : "Valid until")}</p>
            </div>
            <p>You now have full access to all the features included in your package. Visit your dashboard to start using the service.</p>
        </div>
        <div class=""footer"">
            <p>© 2025 WorkSmart. All rights reserved.</p>
            <p>Questions? Contact our support team at support@worksmart.com</p>
        </div>
    </div>
</body>
</html>"
                    };

                    await _sendMailService.SendMail(successEmailContent);

                    await transaction.CommitAsync();

                    return Ok(new
                    {
                        status = "SUCCESS",
                        message = "Payment and subscription created successfully",
                        orderCode = orderCode,
                        packageName = package.Name
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw; // Re-throw để xử lý ở catch bên ngoài
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "An error occurred while processing payment",
                    Details = ex.Message
                });
            }
        }

        [HttpGet("test-payment-fail/{orderCode}")]
        public async Task<IActionResult> TestPaymentFail(long orderCode)
        {
            return await ProcessPaymentReturn(
                code: "01",  // Code không phải "00"
                id: "123123123123",
                cancel: false,
                status: "FAILED",
                orderCode: orderCode
            );
        }

        [HttpGet("payment-cancel")]
        public async Task<IActionResult> CancelPayment([FromQuery] long orderCode, [FromQuery] string code, [FromQuery] string id, [FromQuery] bool cancel, [FromQuery] string status)
        {
            try
            {
                if (code == "00" && status == "PAID" && !cancel)
                {
                    return RedirectToAction("ProcessPaymentReturn", new
                    {
                        code,
                        id,
                        cancel,
                        status,
                        orderCode
                    });
                }

                var existingTransaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.OrderCode == orderCode);

                if (existingTransaction == null)
                {
                    return NotFound("Transaction not found");
                }

                if (existingTransaction.Status == "CANCELLED" || existingTransaction.Status == "FAILED")
                {
                    return Ok(new
                    {
                        status = existingTransaction.Status,
                        message = $"Payment was already processed with status: {existingTransaction.Status}",
                        orderCode = orderCode
                    });
                }

                existingTransaction.Status = "CANCELLED";
                existingTransaction.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                await _signalRService.SendNotificationToUser(
                    existingTransaction.UserID,
                    "Payment Cancelled",
                    $"Your payment for order {existingTransaction.Content} has been cancelled."
                );

                return Ok(new
                {
                    status = "CANCELLED",
                    message = "Payment has been cancelled",
                    orderCode = orderCode
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "An error occurred while canceling payment",
                    Details = ex.Message
                });
            }
        }

        //[HttpPost("webhook")]
        //public async Task<IActionResult> HandlePaymentWebhook([FromBody] WebhookType body)
        //{
        //    try
        //    {
        //        WebhookData data = _payOS.verifyPaymentWebhookData(body);

        //        var transaction = await _context.Transactions
        //            .FirstOrDefaultAsync(t => t.OrderCode == data.orderCode);

        //        if (transaction == null)
        //        {
        //            return NotFound("Payment not found");
        //        }

        //        var package = await _context.Packages
        //            .FirstOrDefaultAsync(p => p.Name == transaction.Content.Replace("Pay ", ""));

        //        bool paymentSuccess = body.success &&
        //                       data.amount > 0 &&
        //                       (data.code == "00" || data.desc.ToLower() == "success");

        //        if (paymentSuccess)
        //        {
        //            transaction.Status = "SUCCESS";

        //            var existingSubscription = await _context.Subscriptions
        //                .FirstOrDefaultAsync(s => s.UserID == transaction.UserID && s.PackageID == package.PackageID);

        //            if (existingSubscription == null)
        //            {
        //                var subscription = new Subscription
        //                {
        //                    UserID = transaction.UserID,
        //                    PackageID = package.PackageID,
        //                    ExpDate = DateTime.Now.AddDays(30), // Mặc định 30 ngày
        //                    CreatedAt = DateTime.Now
        //                };

        //                _context.Subscriptions.Add(subscription);
        //            }
        //        }
        //        else
        //        {
        //            transaction.Status = "FAILED";
        //        }

        //        await _context.SaveChangesAsync();

        //        return Ok("Webhook processed successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Error = "An error occurred while handle webhook", Details = ex.Message });

        //    }
        //}
    }
}