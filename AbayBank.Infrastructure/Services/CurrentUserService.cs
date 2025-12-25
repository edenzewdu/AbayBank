using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using AbayBank.Application.Interfaces;

namespace AbayBank.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService  // Changed from IUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier);
        
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) 
            ? userId 
            : null;
    }

    public string? GetUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.Email)?.Value;
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

    public bool IsAdmin()
    {
        return _httpContextAccessor.HttpContext?.User?
            .IsInRole("Admin") ?? false;
    }

    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?
            .IsInRole(role) ?? false;
    }

    public IEnumerable<string> GetUserRoles()
    {
        return _httpContextAccessor.HttpContext?.User?
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value) ?? Enumerable.Empty<string>();
    }

    public ClaimsPrincipal? GetCurrentUser()
    {
        return _httpContextAccessor.HttpContext?.User;
    }
}