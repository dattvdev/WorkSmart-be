using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.EmployerDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Application.Mapper
{
    public class EmployerProfile : Profile
    {
        public EmployerProfile()
        {
            CreateMap<User, GetEmployerProfileDto>();
        }
    }
}
