using AOIntuneAlerts.Domain.Aggregates.Alert;
using AOIntuneAlerts.Domain.Aggregates.ComplianceRule;
using AOIntuneAlerts.Domain.Aggregates.Device;
using Microsoft.EntityFrameworkCore;

namespace AOIntuneAlerts.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Device> Devices { get; }
    DbSet<DeviceComplianceIssue> DeviceComplianceIssues { get; }
    DbSet<DeviceBrowser> DeviceBrowsers { get; }
    DbSet<ComplianceRule> ComplianceRules { get; }
    DbSet<VendorSupportDate> VendorSupportDates { get; }
    DbSet<Alert> Alerts { get; }
    DbSet<AlertRecipient> AlertRecipients { get; }
    DbSet<AlertCooldown> AlertCooldowns { get; }
    DbSet<PortalUser> PortalUsers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
