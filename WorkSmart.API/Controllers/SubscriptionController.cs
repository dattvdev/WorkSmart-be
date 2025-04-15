using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.PackageDtos;
using WorkSmart.Core.Dto.SubscriptionDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;
using WorkSmart.Repository.Repository;

namespace WorkSmart.API.Controllers
{
    [Route("subscriptions")]
    [ApiController]
    public class SubscriptionController : Controller
    {
        private readonly SubscriptionService _subscriptionService;
        private readonly PackageService _packageService;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        public SubscriptionController(SubscriptionService subscriptionService, PackageService packageService, IMapper mapper, IAccountRepository accountRepository)
        {
            _subscriptionService = subscriptionService;
            _packageService = packageService;
            _accountRepository = accountRepository;
            _mapper = mapper;
        }

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var subscriptions = await _subscriptionService.GetAll();
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("getByUserId/{id}")]
        public async Task<IActionResult> GetByUserId(int id)
        {
            try
            {
                var subscriptionsWithPackages = await _subscriptionService.GetByUserId(id);
                if (subscriptionsWithPackages == null || !subscriptionsWithPackages.Any())
                    return NotFound(new { message = "No subscriptions found" });

                var validSubscriptions = subscriptionsWithPackages
                    .Select(item => new SubscriptionWithPackage
                    {
                        Subscription = item.subscription,
                        Package = item.package
                    })
                    .ToList();

                if (!validSubscriptions.Any())
                    return BadRequest(new { message = "All subscriptions expired" });

                return Ok(validSubscriptions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _subscriptionService.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] SubscriptionDto subscriptionDto)
        {
            try
            {
                await _subscriptionService.Add(subscriptionDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] SubscriptionDto subscriptionDto)
        {
            try
            {
                _subscriptionService.Update(subscriptionDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("SubscriptionRevenueDashboard")]
        public async Task<IActionResult> SubscriptionRevenueDashboard()
        {
            try
            {
                var data = await _subscriptionService.SubscriptionRevenueDashboard();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("has-active-subscription/{userId}")]
        public async Task<IActionResult> HasActiveSubscription(int userId)
        {
            try
            {
                var user = await _accountRepository.GetById(userId);
                var subscriptionsWithPackages = await _subscriptionService.GetByUserId(userId);
                // Lọc ra các subscription vẫn còn hiệu lực
                var activeSubscriptions = subscriptionsWithPackages
                    .Where(item => item.subscription.ExpDate > DateTime.Now)
                    .ToList();

                if (activeSubscriptions.Any())
                {
                    // Định nghĩa thứ tự ưu tiên của các gói theo loại user
                    var packagePriority = new Dictionary<string, int>();

                    if (user.Role == "Employer")
                    {
                        packagePriority = new Dictionary<string, int>
                        {
                            { "Employer Premium", 3 },
                            { "Employer Standard", 2 },
                            { "Employer Basic", 1 }
                        };
                    }
                    else if (user.Role == "Candidate")
                    {
                        packagePriority = new Dictionary<string, int>
                        {
                            { "Candidate Pro", 3 },
                            { "Candidate Plus", 2 },
                            { "Candidate Basic", 1 }
                        };
                    }

                    // Sắp xếp theo mức độ của gói (cao nhất lên đầu)
                    var highestSubscription = activeSubscriptions
                        .OrderByDescending(item => packagePriority.ContainsKey(item.package.Name)
                            ? packagePriority[item.package.Name]
                            : 0)
                        .First();

//                    var expiryEmailContent = new Core.Dto.MailDtos.MailContent
//                    {
//                        To = user.Email,
//                        Subject = "Your Subscription Package is About to Expire",
//                        Body = $@"
//<!DOCTYPE html>
//<html lang=""en"">
//<head>
//    <meta charset=""UTF-8"">
//    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
//    <title>Subscription Package Expiring Soon</title>
//    <style>
//        body {{
//            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
//            line-height: 1.6;
//            color: #333;
//            margin: 0;
//            padding: 0;
//            background-color: #f9f9f9;
//        }}
//        .email-container {{
//            max-width: 600px;
//            margin: 0 auto;
//            background-color: #ffffff;
//            border-radius: 8px;
//            overflow: hidden;
//            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
//        }}
//        .header {{
//            background-color: #FF9800;
//            color: white;
//            padding: 20px;
//            text-align: center;
//        }}
//        .content {{
//            padding: 30px;
//        }}
//        .message {{
//            background-color: #fff3e0;
//            border-left: 4px solid #FF9800;
//            padding: 15px;
//            margin-bottom: 20px;
//            border-radius: 4px;
//        }}
//        .subscription-details {{
//            background-color: #f5f5f5;
//            padding: 15px;
//            border-radius: 4px;
//            margin-bottom: 20px;
//        }}
//        .cta-button {{
//            display: inline-block;
//            background-color: #FF9800;
//            color: white;
//            text-decoration: none;
//            padding: 12px 25px;
//            border-radius: 4px;
//            font-weight: bold;
//            margin: 20px 0;
//        }}
//        .package-options {{
//            display: flex;
//            justify-content: space-between;
//            flex-wrap: wrap;
//            margin: 20px 0;
//        }}
//        .package {{
//            border: 1px solid #ddd;
//            border-radius: 4px;
//            padding: 15px;
//            width: 30%;
//            text-align: center;
//            margin-bottom: 15px;
//        }}
//        .package-name {{
//            font-weight: bold;
//            font-size: 18px;
//            margin-bottom: 10px;
//        }}
//        .footer {{
//            background-color: #f5f5f5;
//            padding: 20px;
//            text-align: center;
//            font-size: 12px;
//            color: #777;
//        }}
//        @media only screen and (max-width: 600px) {{
//            .package {{
//                width: 100%;
//                margin-bottom: 15px;
//            }}
//        }}
//    </style>
//</head>
//<body>
//    <div class=""email-container"">
//        <div class=""header"">
//            <h2>Subscription Package Expiring Soon</h2>
//        </div>
//        <div class=""content"">
//            <h1 style=""color: #FF9800; text-align: center;"">Don't Let Your Service Be Interrupted!</h1>
//            <div class=""message"">
//                <p>Dear {user.FullName},</p>
//                <p>We would like to inform you that your {highestSubscription.package.Name} subscription package will expire on <strong>{highestSubscription.subscription.ExpDate.ToString("dd/MM/yyyy HH:mm:ss")}</strong>.</p>
//            </div>
//            <div class=""subscription-details"">
//                <h3>Current Subscription Details:</h3>
//                <p><strong>Package:</strong> {highestSubscription.package.Name}</p>
//                <p><strong>Start Date:</strong> {highestSubscription.subscription.CreatedAt.ToString("dd/MM/yyyy")}</p>
//                <p><strong>Expiration Date:</strong> {highestSubscription.subscription.ExpDate.ToString("dd/MM/yyyy HH:mm:ss")}</p>
//                <p><strong>Time Remaining:</strong> {(highestSubscription.subscription.ExpDate - DateTime.Now).Days} days</p>
//            </div>
//            <p>To continue enjoying full access to WorkSmart's features and services, please renew your subscription package or upgrade to a higher tier for an enhanced experience.</p>
            
//            <p>If you allow your subscription to expire, certain features will be restricted and you may not have full access to our services.</p>
            
//            <p>Thank you for your trust and continued use of WorkSmart's services!</p>
//        </div>
//        <div class=""footer"">
//            <p>© 2025 WorkSmart. All rights reserved.</p>
//            <p>Need assistance? Contact our support team at support@worksmart.com</p>
//        </div>
//    </div>
//</body>
//</html>"
//                    };

                    return Ok(new
                    {
                        HasActiveSubscription = true,
                        ActiveSubscription = highestSubscription.subscription,
                        Package = highestSubscription.package,
                        ExpireDate = highestSubscription.subscription.ExpDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        RemainingSubscriptionsCount = activeSubscriptions.Count - 1
                    });
                }
                return Ok(new
                {
                    HasActiveSubscription = false,
                    Message = "No active subscription found. User can purchase a new package."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while checking subscription status", Details = ex.Message });
            }
        }
    }
}
