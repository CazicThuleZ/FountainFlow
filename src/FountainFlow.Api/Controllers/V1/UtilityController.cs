using FountainFlow.Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FountainFlow.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class UtilityController : ControllerBase
    {
        private readonly FFDbContext _context;

        public UtilityController(FFDbContext context)
        {
            _context = context;
        }

        [HttpGet("health-check")]
        public IActionResult HealthCheck()
        {
            return Ok("API is up and running");
        }

    }
}
