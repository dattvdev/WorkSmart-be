using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.UserDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface INotificationJobTagRepository : IBaseRepository<NotificationJobTag>
    {
        Task<List<UserNotificationTagDto>> GetNotiUserByListTagID(List<int> tagIDs);
        Task<List<UserNotificationTagManageDto>> GetListRegisterTag(int UserID);
        Task AddNotificationTag(int userId, List<int> tagId, string email);
    }
}
