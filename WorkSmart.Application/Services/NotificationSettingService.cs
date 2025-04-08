using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.NotificationSettingDtos;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Application.Services
{
    public class NotificationSettingService
    {
        private readonly INotificationSettingRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public NotificationSettingService(INotificationSettingRepository repository, IMapper mapper, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<object?> GetByIdAsync(int id, string role)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null) return null;
            
            if (user.NotificationSetting == null) return null;

            return role.ToLower() switch
            {
                "candidate" => _mapper.Map<CandidateNotificationSettingsDto>(user.NotificationSetting),
                "employer" => _mapper.Map<EmployerNotificationSettingsDto>(user.NotificationSetting),
                _ => null
            };
        }

        public async Task<object> CreateAsync(object dto, string role)
        {
            NotificationSetting entity;

            switch (role.ToLower())
            {
                case "candidate":
                    entity = _mapper.Map<NotificationSetting>((CandidateNotificationSettingsDto)dto);
                    break;
                case "employer":
                    entity = _mapper.Map<NotificationSetting>((EmployerNotificationSettingsDto)dto);
                    break;
                default:
                    throw new ArgumentException("Invalid role");
            }

            await _repository.Add(entity);

            return role.ToLower() switch
            {
                "candidate" => _mapper.Map<CandidateNotificationSettingsDto>(entity),
                "employer" => _mapper.Map<EmployerNotificationSettingsDto>(entity),
                _ => throw new Exception("Unknown role")
            };
        }

        public async Task<bool> UpdateAsync(int id, object dto, string role)
        {
            var user = await _userRepository.GetUserById(id);
            var entity = await _repository.GetById(user.NotificationSetting.NotificationSettingID);
            if (entity == null) return false;

            switch (role.ToLower())
            {
                case "candidate":
                    var candidateDto = JsonConvert.DeserializeObject<CandidateNotificationSettingsDto>(dto.ToString());
                    var updatedCandidateDto = _mapper.Map<CandidateNotificationSettingsUpdateDto>(candidateDto);
                    _mapper.Map(updatedCandidateDto, entity);
                    break;
                case "employer":
                    var employerDto = JsonConvert.DeserializeObject<EmployerNotificationSettingsDto>(dto.ToString());
                    var updatedEmployerDto = _mapper.Map<EmployerNotificationSettingsUpdateDto>(employerDto);
                    _mapper.Map(updatedEmployerDto, entity);
                    break;
                default:
                    throw new ArgumentException("Invalid role");
            }

            _repository.Update(entity);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _repository.Delete(id);
            return true;
        }
    }

}
