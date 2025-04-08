using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class NotificationSettingRepository : BaseRepository<NotificationSetting>, INotificationSettingRepository
    {
        public NotificationSettingRepository(WorksmartDBContext context) : base(context)
        {
        }
    }
}
