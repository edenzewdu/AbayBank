using AbayBank.Application.DTOs;
using AbayBank.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AbayBank.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers([FromQuery] UserQuery query)
        {
            try
            {
                var users = await _userService.GetUsersAsync(query);
                return Ok(new ApiResponse<IEnumerable<UserDto>>(true, "Users retrieved", users));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new ApiResponse<string>(false, "User not found"));
                
                return Ok(new ApiResponse<UserDto>(true, "User retrieved", user));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var result = await _userService.CreateUserAsync(request);
                if (!result.Success)
                    return BadRequest(result);
                
                return CreatedAtAction(nameof(GetUser), new { id = result.Data?.Id }, 
                    new ApiResponse<UserDto>(true, "User created successfully", result.Data));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (id != currentUserId && !User.IsInRole("Admin"))
                    return Forbid();
                
                var result = await _userService.UpdateUserAsync(id, request);
                if (!result.Success)
                    return BadRequest(result);
                
                return Ok(new ApiResponse<UserDto>(true, "User updated successfully", result.Data));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (!result.Success)
                    return BadRequest(result);
                
                return Ok(new ApiResponse<bool>(true, "User deleted successfully", result.Data));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _userService.ChangePasswordAsync(userId, request);
                if (!result.Success)
                    return BadRequest(result);
                
                return Ok(new ApiResponse<bool>(true, "Password changed successfully", result.Data));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string>(false, ex.Message));
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User not authenticated");
            
            if (!Guid.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid user ID format");
            
            return userId;
        }
    }
}