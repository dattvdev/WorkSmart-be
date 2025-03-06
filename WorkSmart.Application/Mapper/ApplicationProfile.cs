using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Application.Mapper
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            CreateMap<Application, ApplicationJobDto>();
        }
    }
}
