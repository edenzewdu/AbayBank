namespace AbayBank.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? GetUserId();
    string? GetUserEmail();
    bool IsAuthenticated();
    bool IsAdmin();
    IEnumerable<string> GetUserRoles();
}