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
        
    }
}
