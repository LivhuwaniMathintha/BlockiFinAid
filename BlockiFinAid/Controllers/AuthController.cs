using System.Net;
using BlockiFinAid.Data.DTOs;
using BlockiFinAid.Services.AccessControl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlockiFinAid.Controllers
{
    [Route("api/[controller]/")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserAccessControlService _authService;
        private readonly ILogger<UserAccessControlService> _logger;

        public AuthController(UserAccessControlService authService, ILogger<UserAccessControlService> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var results = await _authService.LoginUser(request.Username, request.Password);

            if (results is not null)
            {
                return Ok(new LoginResponse
                {
                    Role = results.Role,
                //    Username = results.Username,
                    IsLoginSuccessful = true,
                    StudentNumber = results.StudentNumber,
                    Id = results.Id.ToString(),
                });
            }
            return Unauthorized(new LoginResponse
            {
                Role = "",
                IsLoginSuccessful = false
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserAccessControlDto user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var results = await _authService.RegisterUser(user);
            if (results is not null)
            {
                return StatusCode(201, new LoginResponse
                {
                    IsLoginSuccessful = true,
                    Role = results.Role,
                    Permissions = results.Permissions
                });
            }
            return BadRequest(new LoginResponse());
        }
    }

    public class LoginResponse
    {
        public bool IsLoginSuccessful { get; set; }
        public string Role { get; set; } = string.Empty;
        public IEnumerable<string> Permissions { get; set; } =  new List<string>();
        public string StudentNumber { get; set; }
        public string Id { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
