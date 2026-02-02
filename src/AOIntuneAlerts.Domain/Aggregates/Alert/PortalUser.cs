using AOIntuneAlerts.Domain.Common;
using AOIntuneAlerts.Domain.Enums;
using AOIntuneAlerts.Domain.ValueObjects;

namespace AOIntuneAlerts.Domain.Aggregates.Alert;

public class PortalUser : Entity<Guid>
{
    public string AzureAdObjectId { get; private set; } = string.Empty;
    public string UserPrincipalName { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public EmailAddress Email { get; private set; } = null!;
    public PortalUserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private PortalUser() { }

    private PortalUser(
        Guid id,
        string azureAdObjectId,
        string userPrincipalName,
        string displayName,
        EmailAddress email,
        PortalUserRole role)
        : base(id)
    {
        AzureAdObjectId = azureAdObjectId;
        UserPrincipalName = userPrincipalName;
        DisplayName = displayName;
        Email = email;
        Role = role;
        IsActive = true;
    }

    public static PortalUser Create(
        string azureAdObjectId,
        string userPrincipalName,
        string displayName,
        string email,
        PortalUserRole role = PortalUserRole.Viewer)
    {
        if (string.IsNullOrWhiteSpace(azureAdObjectId))
            throw new ArgumentException("Azure AD Object ID is required", nameof(azureAdObjectId));

        var emailAddress = EmailAddress.Create(email);
        return new PortalUser(
            Guid.NewGuid(),
            azureAdObjectId,
            userPrincipalName,
            displayName,
            emailAddress,
            role);
    }

    public void UpdateRole(PortalUserRole role)
    {
        Role = role;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanManageAlerts()
    {
        return Role >= PortalUserRole.Manager;
    }

    public bool CanManageRules()
    {
        return Role >= PortalUserRole.Admin;
    }

    public bool CanTriggerSync()
    {
        return Role >= PortalUserRole.Admin;
    }
}
