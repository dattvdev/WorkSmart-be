using Microsoft.AspNetCore.Mvc;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.NotificationJobTagDtos;

namespace WorkSmart.API.Controllers
{

    [Route("notificationJobTag")]
    [ApiController]
    public class NotificationJobTagController : Controller
    {
        private readonly NotificationJobTagService _notificationJobTagService;

        public NotificationJobTagController(NotificationJobTagService notificationJobTagService )
        {
            _notificationJobTagService = notificationJobTagService;
        }
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotificationTags(int userId)
        {
            var tags = await _notificationJobTagService.GetListRegisterTag(userId);
            return Ok(tags);
        }

        [HttpPost]
        public async Task<IActionResult> AddNotificationTag([FromBody] AddNotificationTagRequest request)
        {
            await _notificationJobTagService.AddNotificationTag(request.UserId, request.TagId, request.Email);
            return Ok();
        }

        [HttpDelete("delete/by-category/{userId}/{categoryID}")]
        public async Task<IActionResult> DeleteNotificationTag(int userId, string categoryID)
        {
            _notificationJobTagService.DeleteByCategory(userId, categoryID);
            return Ok();
        }
        [HttpDelete("delete/by-category-email/{userId}/{categoryID}/{email}")]
        public async Task<IActionResult> DeleteNotificationTag(int userId, string categoryID, string email)
        {
            _notificationJobTagService.DeleteByCategoryEmail(userId, categoryID, email);
            return Ok();
        }

        [HttpDelete("delete/by-category-email-tag/{userId}/{categoryID}/{email}/{tagIds}")]
        public async Task<IActionResult> DeleteNotificationTag(int userId, string categoryID, string email, int tagIds)
        {
            _notificationJobTagService.DeleteByCategoryEmailTag(userId, categoryID, email, tagIds);
            return Ok();
        }


        //GetListTagIdByEmail
        [HttpGet("tagId/{userId}/{email}")]
        public async Task<IActionResult> GetListTagIdByEmail(int userId, string email)
        {
            var tags = await _notificationJobTagService.GetListTagIdByEmail(userId, email);
            return Ok(tags);
        }
    }
}
