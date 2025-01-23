using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using WorkSmart.Core.Dto.UserDto;
using WorkSmart.Core.Entity;
using WorkSmart.Core.Interface;

namespace WorkSmart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserRepository userRepository) : ControllerBase
    {

        // GET: api/User
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok("Get All Users");
        }

        // GET: api/User/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<GetUserDto> Get(int id)
        {
            var user = await userRepository.GetById(id);
            return user as GetUserDto;
        }

        // POST: api/User
        [HttpPost]
        public IActionResult Post([FromBody] string value)
        {
            return Ok("Add User");
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] string value)
        {
            return Ok("Update User");
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return Ok("Delete User");
        }
    }
}
