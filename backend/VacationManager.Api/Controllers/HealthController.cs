using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VacationManager.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Get API welcome information and health status
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public ActionResult<object> Welcome()
        {
            return Ok(new
            {
                message = "Welcome to Vacation Manager API",
                version = "1.0.0",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                documentation = "/swagger/index.html"
            });
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [AllowAnonymous]
        [HttpGet("health")]
        public ActionResult<object> Health()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
