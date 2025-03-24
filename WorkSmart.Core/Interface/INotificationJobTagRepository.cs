using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface INotificationJobTagRepository : IBaseRepository<NotificationJobTag>
    {
        Task<List<string>> GetUserIDByTagID(List<int> tagIDs);
    }
}
