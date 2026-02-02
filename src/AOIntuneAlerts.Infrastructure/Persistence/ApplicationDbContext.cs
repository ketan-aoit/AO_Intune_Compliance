using AOIntuneAlerts.Application.Common.Interfaces;
using AOIntuneAlerts.Domain.Aggregates.Alert;
using AOIntuneAlerts.Domain.Aggregates.ComplianceRule;
using AOIntuneAlerts.Domain.Aggregates.Device;
using Microsoft.EntityFrameworkCore;

namespace AOIntuneAlerts.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceComplianceIssue> DeviceComplianceIssues => Set<DeviceComplianceIssue>();
    public DbSet<DeviceBrowser> DeviceBrowsers => Set<DeviceBrowser>();
    public DbSet<ComplianceRule> ComplianceRules => Set<ComplianceRule>();
    public DbSet<VendorSupportDate> VendorSupportDates => Set<VendorSupportDate>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AlertRecipient> AlertRecipients => Set<AlertRecipient>();
    public DbSet<AlertCooldown> AlertCooldowns => Set<AlertCooldown>();
    public DbSet<PortalUser> PortalUsers => Set<PortalUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
