using AbayBank.Application.DTOs;
using AbayBank.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AbayBank.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IUserService _userService;

        // Only ONE constructor with ALL dependencies
        public AuthController(ILogger<AuthController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Data = (string)null,
                        Message = "Invalid request data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var result = await _userService.RegisterAsync(request);
                
                if (!result.Success)
                {
                    // Return the same structure as the error you're seeing
                    return BadRequest(new
                    {
                        Success = false,
                        Data = (string)null,
                        Message = "Registration failed",
                        Errors = result.Errors ?? new List<string>()
                    });
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception here (using ILogger)
                _logger.LogError(ex, "Error during registration");
                
                return BadRequest(new
                {
                    Success = false,
                    Data = (string)null,
                    Message = "An error occurred during registration",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Data = (string)null,
                        Message = "Invalid request data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var result = await _userService.LoginAsync(request);
                
                if (!result.Success)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Data = (string)null,
                        Message = result.Message,
                        Errors = result.Errors ?? new List<string>()
                    });
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return BadRequest(new
                {
                    Success = false,
                    Data = (string)null,
                    Message = "An error occurred during login",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim?.Value))
                    return Unauthorized(new
                    {
                        Success = false,
                        Data = (string)null,
                        Message = "User not authenticated"
                    });
                
                var user = await _userService.GetUserByIdAsync(Guid.Parse(userIdClaim.Value));
                if (user == null)
                    return NotFound(new
                    {
                        Success = false,
                        Data = (string)null,
                        Message = "User not found"
                    });
                
                return Ok(new
                {
                    Success = true,
                    Data = user,
                    Message = "Profile retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return BadRequest(new
                {
                    Success = false,
                    Data = (string)null,
                    Message = "An error occurred while retrieving profile",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}