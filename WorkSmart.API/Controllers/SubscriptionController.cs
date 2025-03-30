using Microsoft.AspNetCore.Mvc;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.SubscriptionDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.API.Controllers
{
    [Route("subscriptions")]
    [ApiController]
    public class SubscriptionController : Controller
    {
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionController(SubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
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
    }
}
