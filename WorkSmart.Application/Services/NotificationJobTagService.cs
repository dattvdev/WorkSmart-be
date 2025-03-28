using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.UserDtos;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class NotificationJobTagService
    {
        private readonly INotificationJobTagRepository _notificationJobTagRepository;
        public NotificationJobTagService(INotificationJobTagRepository notificationJobTagRepository) {
            _notificationJobTagRepository = notificationJobTagRepository;
        }

        public async Task<List<UserNotificationTagDto>> GetNotiUserByListTagID(List<int> tagIDs)
        {
            var list =  await _notificationJobTagRepository.GetNotiUserByListTagID(tagIDs);
            return list;
        }

        public async Task<List<UserNotificationTagManageDto>> GetListRegisterTag(int UserID)
        {
            var list = await _notificationJobTagRepository.GetListRegisterTag(UserID);
            return list;
        }

        public async Task AddNotificationTag(int userId, List<int> tagId, string email)
        {
            await _notificationJobTagRepository.AddNotificationTag(userId, tagId, email);
        }
        public void DeleteByCategory(int userId, string categoryID)
        {
            _notificationJobTagRepository.DeleteByCategory(userId, categoryID);
        }
        public void DeleteByCategoryEmail(int userId, string categoryID, string email)
        {
            _notificationJobTagRepository.DeleteByCategoryEmail(userId, categoryID, email);
        }
        public void DeleteByCategoryEmailTag(int userId, string categoryID, string email, int tagIds)
        {
            _notificationJobTagRepository.DeleteByCategoryEmailTag(userId, categoryID, email, tagIds);
        }
        public async Task<List<int>> GetListTagIdByEmail(int userId, string email)
        {
            var list = await _notificationJobTagRepository.GetListTagIdByEmail(userId, email);
            return list;
        }
    }
}
