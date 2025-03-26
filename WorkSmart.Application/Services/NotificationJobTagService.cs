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
    }
}
