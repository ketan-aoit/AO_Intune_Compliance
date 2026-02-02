using System.Security.Claims;
using AOIntuneAlerts.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AOIntuneAlerts.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => GetClaimValue(ClaimTypes.NameIdentifier) ?? GetClaimValue("oid");

    public string? UserName => GetClaimValue(ClaimTypes.Name) ?? GetClaimValue("name");

    public string? Email => GetClaimValue(ClaimTypes.Email) ?? GetClaimValue("preferred_username");

    public IEnumerable<string> Roles =>
        _httpContextAccessor.HttpContext?.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "roles")
            .Select(c => c.Value)
        ?? Enumerable.Empty<string>();

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role)
    {
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    private string? GetClaimValue(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User?.Claims
            .FirstOrDefault(c => c.Type == claimType)?.Value;
    }
}
