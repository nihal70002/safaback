using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PrivateECommerce.API.Controllers
{
    [ApiController]
    [Route("api/test")]
    [Authorize] // 🔐 protected
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("JWT is working 🔥");
        }
    }
}
