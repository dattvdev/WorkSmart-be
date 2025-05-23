﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Helpers;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(WorksmartDBContext context) : base(context)
        {
        }

        public async Task<Notification> CreateNotification(Notification notification)
        {
            try
            {
                var addNoti = new Notification
            {
                UserID = notification.UserID,
                Title = notification.Title,
                Message = notification.Message,
                Link = notification.Link,
                CreatedAt = TimeHelper.GetVietnamTime()
            };
            _dbSet.Add(addNoti);
            await _context.SaveChangesAsync();

            notification.NotificationID = addNoti.NotificationID;
            notification.CreatedAt = addNoti.CreatedAt;

            return notification;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public async Task<IEnumerable<Notification>> GetByUserId(int userId)
        {
            return await _dbSet.Where(x => x.UserID == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadNotificationsCount(int userId)
        {
           return await _dbSet.CountAsync(x => x.UserID == userId && x.IsRead == false);
        }

        public async Task<bool> MarkNotificationAsRead(int notificationId)
        {
            var notification = _context.Notifications.Find(notificationId);
            if (notification == null)
                return false;

            notification.IsRead = true;
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Notification>> GetCandidateJobNotifications(int userId)
        {
            var query = _dbSet.Where(n => n.UserID == userId);

            query = query.Where(n =>
                n.Title.Contains("Saved Job") ||
                n.Title.Contains("Recommended Job") ||
                n.Title.Contains("Application Approved") ||
                n.Title.Contains("Application Applied") ||
                n.Title.Contains("Application Rejected") ||
                n.Title.Contains("Application Deadline")
            );

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}
