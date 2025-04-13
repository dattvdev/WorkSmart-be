using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.NotificationDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class NotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;
        public NotificationService(INotificationRepository notificationRepository, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }
        public async Task<GetNotificationDto> GetNotificationById(int id)
        {
            var notification = await _notificationRepository.GetById(id);
            return _mapper.Map<GetNotificationDto>(notification);
        }
        public async Task<IEnumerable<GetNotificationDto>> GetByUserId(int id)
        {
            var notification = await _notificationRepository.GetByUserId(id);
            return _mapper.Map<IEnumerable<GetNotificationDto>>(notification);
        }

        public async Task CreateNotification(CreateNotificationDto createNotificationDto)
        {
            var notification = _mapper.Map<Notification>(createNotificationDto);
            await _notificationRepository.Add(notification);
        }

        public void DeleteNotification(int id)
        {
            _notificationRepository.Delete(id);
        }

        public async Task<GetNotificationDto> CreateNotification(GetNotificationDto notificationDto)
        {
            var noti = await _notificationRepository.CreateNotification(_mapper.Map<Notification>(notificationDto));
           return _mapper.Map<GetNotificationDto>(noti);
        }
        public async Task<int> GetUnreadNotificationsCount(int userId)
        {
            return await _notificationRepository.GetUnreadNotificationsCount(userId);
        }
        public async Task<bool> MarkNotificationAsRead(int notificationId)
        {
            return await _notificationRepository.MarkNotificationAsRead(notificationId);
        }
        public async Task CreateJobExpiringNotification(Job job, int userId, int daysRemaining)
        {
            var notification = new CreateNotificationDto
            {
                UserID = userId,
                Title = "Job sắp hết hạn",
                Message = $"Job sẽ hết hạn trong {daysRemaining} ngày.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                Link = $"/jobs/detail/{job.JobID}" // Giả sử Job có thuộc tính JobID
            };

            await CreateNotification(notification);
        }

        public async Task CreateJobExpiredNotification(Job job, int userId)
        {
            var notification = new CreateNotificationDto
            {
                UserID = userId,
                Title = "Job đã hết hạn",
                Message = $"Job đã hết hạn 1 ngày trước.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                Link = $"/jobs/detail/{job.JobID}" // Giả sử Job có thuộc tính JobID
            };

            await CreateNotification(notification);
        }


    } 
}