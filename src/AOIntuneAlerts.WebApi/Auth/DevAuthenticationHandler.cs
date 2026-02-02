using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace AOIntuneAlerts.WebApi.Auth;

/// <summary>
/// Development-only authentication handler that bypasses Azure AD.
/// WARNING: This should NEVER be enabled in production!
/// </summary>
public class DevAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "DevAuth";
    public const string DevAuthHeader = "X-Dev-Auth";
    private readonly IConfiguration _configuration;

    public DevAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check for dev auth header
        if (!Request.Headers.TryGetValue(DevAuthHeader, out var devAuthValue))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Validate the dev auth secret
        var expectedSecret = _configuration["DevAuth:Secret"];
        if (string.IsNullOrEmpty(expectedSecret) || devAuthValue != expectedSecret)
        {
            Logger.LogWarning("Invalid dev auth secret provided");
            return Task.FromResult(AuthenticateResult.Fail("Invalid dev auth secret"));
        }

        // Get test user info from headers or use defaults
        var userName = Request.Headers["X-Dev-User"].FirstOrDefault() ?? "test@example.com";
        var userRole = Request.Headers["X-Dev-Role"].FirstOrDefault() ?? "Admin";

        // Create claims for the test user
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "dev-user-001"),
            new(ClaimTypes.Name, userName),
            new("name", userName),
            new(ClaimTypes.Email, userName),
            new("preferred_username", userName),
            new("roles", userRole),
            new("oid", "00000000-0000-0000-0000-000000000001"),
            new("tid", "00000000-0000-0000-0000-000000000000"),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        Logger.LogInformation("Dev auth: Authenticated as {User} with role {Role}", userName, userRole);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
