using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.NotificationSettingDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Application.Mapper
{
    public class NotificationSettingProfile : Profile
    {
        public NotificationSettingProfile()
        {
            CreateMap<NotificationSetting, CandidateNotificationSettingsDto>().ReverseMap();
            CreateMap<NotificationSetting, EmployerNotificationSettingsDto>().ReverseMap();
            CreateMap<NotificationSetting, CandidateNotificationSettingsUpdateDto>().ReverseMap();
            CreateMap<NotificationSetting, EmployerNotificationSettingsUpdateDto>().ReverseMap();
            CreateMap<CandidateNotificationSettingsDto, CandidateNotificationSettingsUpdateDto>().ReverseMap();
            CreateMap<EmployerNotificationSettingsDto, EmployerNotificationSettingsUpdateDto>().ReverseMap();
        }
    }
   
}
