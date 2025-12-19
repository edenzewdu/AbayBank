using AbayBank.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AbayBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IConfiguration configuration,
        IUserService userService,
        ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _userService.RegisterAsync(request);
            return Ok(new { success = true, message = "Registration successful", data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _userService.AuthenticateAsync(request.Email, request.Password);
            
            if (user == null)
                return Unauthorized(new { success = false, message = "Invalid credentials" });

            var token = GenerateJwtToken(user);
            
            return Ok(new { 
                success = true, 
                message = "Login successful", 
                token,
                user = new {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = user.Role
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // In a real app, you might blacklist the token
        return Ok(new { success = true, message = "Logout successful" });
    }

    private string GenerateJwtToken(UserDto user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

// AbayBank.Application/DTOs/AuthDtos.cs
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = "User";
}