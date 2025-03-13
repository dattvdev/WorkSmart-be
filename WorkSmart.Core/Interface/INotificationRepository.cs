using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserId(int userId);
        Task<int> GetUnreadNotificationsCount(int userId);
        Task<bool> MarkNotificationAsRead(int notificationId);
        Task<Notification> CreateNotification(Notification notification);
    }
}
