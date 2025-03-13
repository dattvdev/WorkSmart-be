using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.ApplicationDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Application.Mapper
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {

            CreateMap<Core.Entity.Application, ApplicationJobDto>()
                .ForMember(u => u.FullName, option => option.MapFrom(a => a.User.FullName))
                .ForMember(u => u.PhoneNumber, option => option.MapFrom(a => a.User.PhoneNumber))
                .ForMember(u => u.Email, option => option.MapFrom(a => a.User.Email));


        }
    }
}
