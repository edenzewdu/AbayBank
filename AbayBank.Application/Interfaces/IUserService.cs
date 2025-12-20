using AbayBank.Application.DTOs;

namespace AbayBank.Application.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<IEnumerable<UserDto>> GetUsersAsync(UserQuery? query = null);
    Task<ServiceResult<UserDto>> CreateUserAsync(CreateUserRequest request);
    Task<ServiceResult<UserDto>> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<ServiceResult<bool>> DeleteUserAsync(Guid id);
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ServiceResult<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}