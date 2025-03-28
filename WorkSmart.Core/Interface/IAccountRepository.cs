﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.AdminDtos;
using WorkSmart.Core.Entity;

namespace WorkSmart.Core.Interface
{
    public interface IAccountRepository : IBaseRepository<User>
    {
        User GetByEmail(string email);
        bool UserExist(string email);
    }
}
