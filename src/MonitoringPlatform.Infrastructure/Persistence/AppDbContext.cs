using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MonitoringPlatform.Infrastructure.Persistence.Identity;
using CloudIncidentEntity = MonitoringPlatform.Domain.Entities.CloudIncident;
using CloudProviderEntity = MonitoringPlatform.Domain.Entities.CloudProvider;
using MonitorEntity = MonitoringPlatform.Domain.Entities.Monitor;
using MonitorLogEntity = MonitoringPlatform.Domain.Entities.MonitorLog;
using MicrosoftIntegrationEntity = MonitoringPlatform.Domain.Entities.MicrosoftIntegration;
using TenantEntity = MonitoringPlatform.Domain.Entities.Tenant;
using AlertRuleEntity = MonitoringPlatform.Domain.Entities.AlertRule;
using AlertHistoryEntity = MonitoringPlatform.Domain.Entities.AlertHistory;
using TenantSettingsEntity = MonitoringPlatform.Domain.Entities.TenantSettings;
using UserAlertPreferenceEntity = MonitoringPlatform.Domain.Entities.UserAlertPreference;

namespace MonitoringPlatform.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<MonitorEntity> Monitors { get; set; }
        public DbSet<MonitorLogEntity> MonitorLogs { get; set; }
        public DbSet<CloudProviderEntity> CloudProviders { get; set; }
        public DbSet<CloudIncidentEntity> CloudIncidents { get; set; }
        public DbSet<TenantEntity> Tenants { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<MicrosoftIntegrationEntity> MicrosoftIntegrations => Set<MicrosoftIntegrationEntity>();
        public DbSet<AlertRuleEntity> AlertRules => Set<AlertRuleEntity>();
        public DbSet<AlertHistoryEntity> AlertHistories => Set<AlertHistoryEntity>();
        public DbSet<TenantSettingsEntity> TenantSettings => Set<TenantSettingsEntity>();
        public DbSet<UserAlertPreferenceEntity> UserAlertPreferences => Set<UserAlertPreferenceEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(120);
                entity.Property(e => e.TenantId).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TenantEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(80);
                entity.Property(e => e.CreatedAtUtc).IsRequired();
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TokenHash).IsUnique();
                entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(256);
                entity.Property(e => e.ExpiresAt).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.ReplacedByTokenHash).HasMaxLength(256);
                entity.HasOne(e => e.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MonitorEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TenantId).IsRequired();
                entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
                entity.Property(e => e.IntervalInSeconds).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.HasIndex(e => new { e.TenantId, e.Name });
                entity.HasOne(e => e.Tenant)
                      .WithMany(t => t.Monitors)
                      .HasForeignKey(e => e.TenantId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Logs)
                      .WithOne(l => l.Monitor)
                      .HasForeignKey(l => l.MonitorId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MonitorLogEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.CheckedAt).IsRequired();
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            });

            modelBuilder.Entity<CloudProviderEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TenantId).IsRequired();
                entity.HasIndex(e => new { e.TenantId, e.Slug }).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(80);
                entity.Property(e => e.LogoUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.SourceType).IsRequired();
                entity.Property(e => e.SourceUrl).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.StatusPageUrl).HasMaxLength(500);
                entity.Property(e => e.MetadataJson);
                entity.Property(e => e.IsEnabled).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.Property(e => e.LastSyncError).HasMaxLength(2000);
                entity.HasOne(e => e.Tenant)
                    .WithMany(t => t.CloudProviders)
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.Incidents)
                    .WithOne(i => i.CloudProvider)
                    .HasForeignKey(i => i.CloudProviderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CloudIncidentEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.CloudProviderId, e.ExternalId }).IsUnique();
                entity.HasIndex(e => new { e.CloudProviderId, e.IsActive });
                entity.Property(e => e.ExternalId).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Severity).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Region).HasMaxLength(200);
                entity.Property(e => e.AffectedServicesJson);
                entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
                entity.Property(e => e.OfficialUrl).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.OccurredAt).IsRequired();
                entity.Property(e => e.LastUpdatedAt).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
            });

            modelBuilder.Entity<MicrosoftIntegrationEntity>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.MicrosoftTenantId)
                .HasMaxLength(120)
                .IsRequired();

                entity.Property(x => x.ClientId)
                .HasMaxLength(120)
                .IsRequired();

                entity.Property(x => x.ClientSecret)
                .HasMaxLength(500)
                .IsRequired();

                entity.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AlertRuleEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.AlertType });
                entity.Property(e => e.Name).IsRequired().HasMaxLength(120);
                entity.Property(e => e.AlertType).IsRequired();
                entity.Property(e => e.Channel).IsRequired();
                entity.Property(e => e.IsEnabled).IsRequired();
                entity.Property(e => e.ThrottleMinutes).IsRequired();
                entity.Property(e => e.RecipientEmails).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.SelectedCloudProviderIds).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AlertHistoryEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TenantId, e.SentAt });
                entity.Property(e => e.AlertType).IsRequired();
                entity.Property(e => e.Channel).IsRequired();
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.RecipientEmail).IsRequired().HasMaxLength(256);
                entity.Property(e => e.SentAt).IsRequired();
                entity.Property(e => e.IsSuccess).IsRequired();
                entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.AlertRule)
                    .WithMany()
                    .HasForeignKey(e => e.AlertRuleId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<TenantSettingsEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TenantId).IsUnique();
                entity.Property(e => e.SmtpHost).IsRequired().HasMaxLength(256);
                entity.Property(e => e.SmtpPort).IsRequired();
                entity.Property(e => e.SmtpUsername).IsRequired().HasMaxLength(256);
                entity.Property(e => e.SmtpPassword).IsRequired().HasMaxLength(500);
                entity.Property(e => e.SenderEmail).IsRequired().HasMaxLength(256);
                entity.Property(e => e.SenderName).IsRequired().HasMaxLength(120);
                entity.Property(e => e.UseSsl).IsRequired();
                entity.Property(e => e.EmailEnabled).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserAlertPreferenceEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.TenantId }).IsUnique();
                entity.Property(e => e.EmailEnabled).IsRequired();
                entity.Property(e => e.MonitorDownAlerts).IsRequired();
                entity.Property(e => e.CloudIncidentCriticalAlerts).IsRequired();
                entity.Property(e => e.CloudIncidentMajorAlerts).IsRequired();
                entity.Property(e => e.SummaryEnabled).IsRequired();
                entity.Property(e => e.SummaryFrequency).IsRequired();
                entity.Property(e => e.SummaryDay).IsRequired();
                entity.Property(e => e.SummaryIncludeMonitors).IsRequired();
                entity.Property(e => e.SummaryIncludeCloud).IsRequired();
                entity.Property(e => e.SelectedCloudProviderIds).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.AdditionalEmails).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
