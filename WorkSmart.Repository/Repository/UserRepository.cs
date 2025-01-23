using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Dto.UserDto;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.Repository.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly WorksmartDBContext _context;
        private readonly IMapper _mapper;
        public UserRepository(WorksmartDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task Add(IUserDto entity)
        {
            var user = _mapper.Map<User>(entity);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IUserDto>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<IUserDto> GetById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return _mapper.Map<GetUserDto>(user);
        }

        public void Update(IUserDto entity)
        {
            var existingUser = _context.Users.FirstOrDefault(b => b.UserID == ((GetUserDto)entity).UserID);
            if (existingUser == null)
                throw new Exception("User not found");
            _mapper.Map(entity, existingUser);
            _context.Users.Update(existingUser);
            _context.SaveChanges();
        }
    }
}
