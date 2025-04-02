using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.PackageDtos;
using WorkSmart.Core.Dto.SubscriptionDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.API.Controllers
{
    [Route("subscriptions")]
    [ApiController]
    public class SubscriptionController : Controller
    {
        private readonly SubscriptionService _subscriptionService;
        private readonly PackageService _packageService;
        private readonly IMapper _mapper;

        public SubscriptionController(SubscriptionService subscriptionService, PackageService packageService, IMapper mapper)
        {
            _subscriptionService = subscriptionService;
            _packageService = packageService;
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
                var subscriptionsWithPackages = await _subscriptionService.GetByUserId(userId);

                // Lọc ra các subscription vẫn còn hiệu lực
                var activeSubscriptions = subscriptionsWithPackages
                    .Where(item => item.subscription.ExpDate > DateTime.Now)
                    .OrderByDescending(item => item.subscription.ExpDate)
                    .ToList();

                if (activeSubscriptions.Any())
                {
                    // Lấy subscription có thời hạn dài nhất
                    var latestSubscription = activeSubscriptions.First();

                    return Ok(new
                    {
                        HasActiveSubscription = true,
                        ActiveSubscription = latestSubscription.subscription,
                        Package = latestSubscription.package,
                        ExpireDate = latestSubscription.subscription.ExpDate.ToString("yyyy-MM-dd HH:mm:ss")
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
