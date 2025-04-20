using Microsoft.AspNetCore.Mvc;

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
            var key = _configuration["Cohere:Key"];
            return Ok(new List<string> { connectionString, key });
        }
    }
}
