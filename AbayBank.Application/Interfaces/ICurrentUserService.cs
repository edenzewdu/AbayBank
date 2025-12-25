using System.Security.Claims;

namespace AbayBank.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? GetUserId();
    string? GetUserEmail();
    bool IsAuthenticated();
    bool IsAdmin();
    bool IsInRole(string role);
    IEnumerable<string> GetUserRoles();
    ClaimsPrincipal? GetCurrentUser();
}