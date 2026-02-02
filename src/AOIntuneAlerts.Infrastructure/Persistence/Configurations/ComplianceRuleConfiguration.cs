using AOIntuneAlerts.Domain.Aggregates.ComplianceRule;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AOIntuneAlerts.Infrastructure.Persistence.Configurations;

public class ComplianceRuleConfiguration : IEntityTypeConfiguration<ComplianceRule>
{
    public void Configure(EntityTypeBuilder<ComplianceRule> builder)
    {
        builder.ToTable("ComplianceRules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.Configuration)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(r => r.RuleType);
        builder.HasIndex(r => r.IsEnabled);
    }
}

public class VendorSupportDateConfiguration : IEntityTypeConfiguration<VendorSupportDate>
{
    public void Configure(EntityTypeBuilder<VendorSupportDate> builder)
    {
        builder.ToTable("VendorSupportDates");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.VersionPattern)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.Notes)
            .HasMaxLength(500);

        builder.OwnsOne(v => v.MinimumVersion, mv =>
        {
            mv.Property(ver => ver.Major).HasColumnName("MinVersionMajor");
            mv.Property(ver => ver.Minor).HasColumnName("MinVersionMinor");
            mv.Property(ver => ver.Patch).HasColumnName("MinVersionPatch");
            mv.Property(ver => ver.PreRelease).HasColumnName("MinVersionPreRelease").HasMaxLength(50);
            mv.Property(ver => ver.Build).HasColumnName("MinVersionBuild").HasMaxLength(50);
        });

        builder.HasIndex(v => new { v.OperatingSystemType, v.VersionPattern });
    }
}
