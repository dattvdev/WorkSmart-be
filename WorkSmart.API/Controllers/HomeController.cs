using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace WorkSmart.API.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet("random")]
        public IActionResult GetRandomNumber()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var connectionString1 = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_DefaultConnection"); // lấy từ tab "Connection strings" trong Azure
            var connectionString2 = _configuration["ConnectionStrings:DefaultConnection"];
            var key = _configuration["Cohere:Key"];
            return Ok(new List<string> { connectionString, connectionString1, connectionString2, key });
        }
    }
}
