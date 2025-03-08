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
        [HttpGet("getById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var (subscription, package) = await _subscriptionService.GetById(id);
                return Ok(new { subscription, package });
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
