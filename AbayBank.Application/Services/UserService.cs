using AbayBank.Application.DTOs;
using AbayBank.Application.Exceptions;
using AbayBank.Application.Interfaces;
using AbayBank.Domain.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AbayBank.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new AppException("Email already registered");

        // Create new user
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Hash password
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("User registered: {Email}", request.Email);

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> AuthenticateAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        
        if (user == null || !user.IsActive)
            return null;

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result != PasswordVerificationResult.Success)
            return null;

        _logger.LogInformation("User authenticated: {Email}", email);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("User not found");
            
        return _mapper.Map<UserDto>(user);
    }
}