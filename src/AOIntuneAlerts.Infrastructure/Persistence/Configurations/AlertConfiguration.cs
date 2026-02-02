using AOIntuneAlerts.Domain.Aggregates.Alert;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AOIntuneAlerts.Infrastructure.Persistence.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("Alerts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.Body)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.ErrorMessage)
            .HasMaxLength(1000);

        builder.HasMany(a => a.Recipients)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "AlertAlertRecipient",
                j => j.HasOne<AlertRecipient>().WithMany().HasForeignKey("AlertRecipientId"),
                j => j.HasOne<Alert>().WithMany().HasForeignKey("AlertId"));

        builder.HasIndex(a => a.DeviceId);
        builder.HasIndex(a => a.SentAt);
        builder.HasIndex(a => a.CreatedAt);
    }
}

public class AlertRecipientConfiguration : IEntityTypeConfiguration<AlertRecipient>
{
    public void Configure(EntityTypeBuilder<AlertRecipient> builder)
    {
        builder.ToTable("AlertRecipients");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.OwnsOne(r => r.Email, e =>
        {
            e.Property(email => email.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(320);
        });

        builder.HasIndex(r => r.IsEnabled);
    }
}

public class AlertCooldownConfiguration : IEntityTypeConfiguration<AlertCooldown>
{
    public void Configure(EntityTypeBuilder<AlertCooldown> builder)
    {
        builder.ToTable("AlertCooldowns");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.AlertType)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => new { c.DeviceId, c.AlertType })
            .IsUnique();
    }
}

public class PortalUserConfiguration : IEntityTypeConfiguration<PortalUser>
{
    public void Configure(EntityTypeBuilder<PortalUser> builder)
    {
        builder.ToTable("PortalUsers");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.AzureAdObjectId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(u => u.AzureAdObjectId)
            .IsUnique();

        builder.Property(u => u.UserPrincipalName)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.OwnsOne(u => u.Email, e =>
        {
            e.Property(email => email.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(320);
        });
    }
}
