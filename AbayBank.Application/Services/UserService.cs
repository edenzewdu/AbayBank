using AbayBank.Application.DTOs;
using AbayBank.Application.Interfaces;
using AbayBank.Domain.Entities;
using AbayBank.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace AbayBank.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;
    private readonly IJwtService _jwtService;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserService> logger,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _jwtService = jwtService;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? MapToDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>> GetUsersAsync(UserQuery? query = null)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            throw;
        }
    }

    public async Task<ServiceResult<UserDto>> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return ServiceResult<UserDto>.FailureResult(
                    "Email already registered",
                    new List<string> { "Email already exists" });
            }

            var passwordHash = HashPassword(request.Password);
            var user = new User(
                request.FirstName,
                request.LastName,
                request.Email,
                passwordHash,
                request.Role);

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                user.Update(request.FirstName, request.LastName, request.PhoneNumber);
            }

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<UserDto>.SuccessResult(
                MapToDto(user),
                "User created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return ServiceResult<UserDto>.FailureResult(
                "An error occurred while creating user",
                new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<UserDto>> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return ServiceResult<UserDto>.FailureResult("User not found");
            }

            if (!string.IsNullOrEmpty(request.FirstName) || 
                !string.IsNullOrEmpty(request.LastName) || 
                !string.IsNullOrEmpty(request.PhoneNumber))
            {
                user.Update(
                    request.FirstName ?? user.FirstName,
                    request.LastName ?? user.LastName,
                    request.PhoneNumber ?? user.PhoneNumber);
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                var existingUser = await _userRepository.GetByEmailAsync(request.Email);
                if (existingUser != null && existingUser.Id != id)
                {
                    return ServiceResult<UserDto>.FailureResult("Email already in use");
                }
                // Note: Email update would require additional logic
            }

            if (!string.IsNullOrEmpty(request.Role))
            {
                user.ChangeRole(request.Role);
            }

            if (request.IsActive.HasValue)
            {
                user.SetActive(request.IsActive.Value);
            }

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<UserDto>.SuccessResult(
                MapToDto(user),
                "User updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {Id}", id);
            return ServiceResult<UserDto>.FailureResult(
                "An error occurred while updating user",
                new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<bool>> DeleteUserAsync(Guid id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return ServiceResult<bool>.FailureResult("User not found");
            }

            // Check if user has accounts
            // Additional business logic here

            await _userRepository.DeleteAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "User deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {Id}", id);
            return ServiceResult<bool>.FailureResult(
                "An error occurred while deleting user",
                new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return ServiceResult<AuthResponse>.FailureResult(
                    "Email already registered",
                    new List<string> { "Email already exists" });
            }

            if (request.Password != request.ConfirmPassword)
            {
                return ServiceResult<AuthResponse>.FailureResult(
                    "Passwords do not match",
                    new List<string> { "Password confirmation failed" });
            }

            var passwordHash = HashPassword(request.Password);
            var user = new User(
                request.FirstName,
                request.LastName,
                request.Email,
                passwordHash,
                "User"); // Default role

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                user.Update(request.FirstName, request.LastName, request.PhoneNumber);
            }

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role);
            var authResponse = new AuthResponse
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddDays(7),
                User = MapToDto(user)
            };

            return ServiceResult<AuthResponse>.SuccessResult(
                authResponse,
                "Registration successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return ServiceResult<AuthResponse>.FailureResult(
                "An error occurred during registration",
                new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return ServiceResult<AuthResponse>.FailureResult(
                    "Invalid email or password",
                    new List<string> { "User not found" });
            }

            if (!user.IsActive)
            {
                return ServiceResult<AuthResponse>.FailureResult(
                    "Account is inactive",
                    new List<string> { "Account disabled" });
            }

            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return ServiceResult<AuthResponse>.FailureResult(
                    "Invalid email or password",
                    new List<string> { "Invalid password" });
            }

            var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role);
            var authResponse = new AuthResponse
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddDays(7),
                User = MapToDto(user)
            };

            return ServiceResult<AuthResponse>.SuccessResult(
                authResponse,
                "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return ServiceResult<AuthResponse>.FailureResult(
                "An error occurred during login",
                new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ServiceResult<bool>.FailureResult("User not found");
            }

            if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return ServiceResult<bool>.FailureResult(
                    "Current password is incorrect",
                    new List<string> { "Invalid current password" });
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return ServiceResult<bool>.FailureResult(
                    "New passwords do not match",
                    new List<string> { "Password confirmation failed" });
            }

            var newPasswordHash = HashPassword(request.NewPassword);
            user.ChangePassword(newPasswordHash);

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "Password changed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return ServiceResult<bool>.FailureResult(
                "An error occurred while changing password",
                new List<string> { ex.Message });
        }
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        var hash = HashPassword(password);
        return hash == storedHash;
    }

    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}