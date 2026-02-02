using AOIntuneAlerts.Domain.Aggregates.Device;
using AOIntuneAlerts.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AOIntuneAlerts.Infrastructure.Persistence.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.IntuneDeviceId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(d => d.IntuneDeviceId)
            .IsUnique();

        builder.Property(d => d.DeviceName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.UserPrincipalName)
            .HasMaxLength(320);

        builder.Property(d => d.UserDisplayName)
            .HasMaxLength(200);

        builder.Property(d => d.SerialNumber)
            .HasMaxLength(100);

        builder.Property(d => d.Model)
            .HasMaxLength(200);

        builder.Property(d => d.Manufacturer)
            .HasMaxLength(200);

        builder.OwnsOne(d => d.OperatingSystem, os =>
        {
            os.Property(o => o.Type).HasColumnName("OsType");
            os.Property(o => o.Name).HasColumnName("OsName").HasMaxLength(100);
            os.Property(o => o.Edition).HasColumnName("OsEdition").HasMaxLength(100);
            os.Property(o => o.BuildNumber).HasColumnName("OsBuildNumber").HasMaxLength(50);

            os.OwnsOne(o => o.Version, v =>
            {
                v.Property(ver => ver.Major).HasColumnName("OsVersionMajor");
                v.Property(ver => ver.Minor).HasColumnName("OsVersionMinor");
                v.Property(ver => ver.Patch).HasColumnName("OsVersionPatch");
                v.Property(ver => ver.PreRelease).HasColumnName("OsVersionPreRelease").HasMaxLength(50);
                v.Property(ver => ver.Build).HasColumnName("OsVersionBuild").HasMaxLength(50);
            });
        });

        builder.HasMany(d => d.ComplianceIssues)
            .WithOne()
            .HasForeignKey(i => i.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Browsers)
            .WithOne()
            .HasForeignKey(b => b.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.ComplianceIssues).AutoInclude(false);
        builder.Navigation(d => d.Browsers).AutoInclude(false);
    }
}

public class DeviceComplianceIssueConfiguration : IEntityTypeConfiguration<DeviceComplianceIssue>
{
    public void Configure(EntityTypeBuilder<DeviceComplianceIssue> builder)
    {
        builder.ToTable("DeviceComplianceIssues");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.RuleId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.RuleName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Description)
            .HasMaxLength(1000);

        builder.HasIndex(i => i.DeviceId);
    }
}

public class DeviceBrowserConfiguration : IEntityTypeConfiguration<DeviceBrowser>
{
    public void Configure(EntityTypeBuilder<DeviceBrowser> builder)
    {
        builder.ToTable("DeviceBrowsers");

        builder.HasKey(b => b.Id);

        builder.OwnsOne(b => b.BrowserInfo, bi =>
        {
            bi.Property(b => b.Type).HasColumnName("BrowserType");
            bi.Property(b => b.Name).HasColumnName("BrowserName").HasMaxLength(100);

            bi.OwnsOne(b => b.Version, v =>
            {
                v.Property(ver => ver.Major).HasColumnName("BrowserVersionMajor");
                v.Property(ver => ver.Minor).HasColumnName("BrowserVersionMinor");
                v.Property(ver => ver.Patch).HasColumnName("BrowserVersionPatch");
                v.Property(ver => ver.PreRelease).HasColumnName("BrowserVersionPreRelease").HasMaxLength(50);
                v.Property(ver => ver.Build).HasColumnName("BrowserVersionBuild").HasMaxLength(50);
            });
        });

        builder.HasIndex(b => b.DeviceId);
    }
}
