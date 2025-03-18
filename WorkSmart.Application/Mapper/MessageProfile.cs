using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.MessageDtos;
using WorkSmart.Core.Entity;
namespace WorkSmart.Application.Mapper
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            CreateMap<MessageDto, PersonalMessage>();
            CreateMap<PersonalMessage, MessageDto>()
                .ForMember(u => u.SenderName, option => option.MapFrom(a => a.Sender.FullName))
                .ForMember(u => u.SenderAvatar, option => option.MapFrom(a => a.Sender.Avatar))
                .ForMember(u => u.ReceiverName, option => option.MapFrom(a => a.Receiver.FullName))
                .ForMember(u => u.ReceiverAvatar, option => option.MapFrom(a => a.Receiver.Avatar));
            CreateMap<SendMessageDto, PersonalMessage>().ReverseMap(); 
        }
    }
}
