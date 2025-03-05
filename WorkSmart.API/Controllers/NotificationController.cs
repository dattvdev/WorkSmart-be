using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.NotificationDtos;

namespace WorkSmart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : Controller
    {
        private readonly NotificationService _notificationService;
        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        // GET: api/GetByUserId
        [HttpGet("{userId}")]
        public async Task<IEnumerable<GetNotificationDto>> Get(int userId)
        {
            return await _notificationService.GetByUserId(userId);
        }
        // POST: api/CreateNotification
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateNotificationDto createNotificationDto)
        {
            await _notificationService.CreateNotification(createNotificationDto);
            return Ok();
        }
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            _notificationService.DeleteNotification(id);
        }
    }
}
