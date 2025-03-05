using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.PackageDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Application.Mapper
{
    public class PackageProfile : Profile
    {
        public PackageProfile()
        {
            CreateMap<Package, GetPackageDto>().ReverseMap();
            CreateMap<Package, CreateUpdatePackageDto>().ReverseMap();
        }
    }
}
