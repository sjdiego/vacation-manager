using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using VacationManager.Api.Filters;
using VacationManager.Api.Services;

namespace VacationManager.Api.Controllers
{
    [ApiController]
    [Route("")]
    [ApiVersionNeutral]
    [DisableApiResponseWrapper]
    public class HealthController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IGitVersionService _gitVersionService;

        public HealthController(IWebHostEnvironment environment, IGitVersionService gitVersionService)
        {
            _environment = environment;
            _gitVersionService = gitVersionService;
        }

        /// <summary>
        /// Get API welcome information and health status
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public ActionResult<object> Welcome()
        {
            var response = new
            {
                message = "Welcome to Vacation Manager API",
                version = $"1.0.0+{_gitVersionService.GetCommitHash()}",
                status = "healthy",
                timestamp = DateTime.UtcNow,
                documentation = _environment.IsDevelopment() ? "/swagger/index.html" : null
            };

            return Ok(response);
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
