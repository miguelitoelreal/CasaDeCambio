using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonitoringPlatform.Application.DTOs;
using MonitoringPlatform.Application.Interfaces;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Infrastructure.Persistence;

namespace MonitoringPlatform.API.Services
{
    public class UserAlertPreferenceService
    {
        private readonly AppDbContext _dbContext;
        private readonly ICurrentUserContext _currentUser;

        public UserAlertPreferenceService(AppDbContext dbContext, ICurrentUserContext currentUser)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
        }

        public async Task<UserAlertPreferenceDto> GetMyPreferencesAsync(CancellationToken cancellationToken = default)
        {
            var pref = await _dbContext.UserAlertPreferences
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, cancellationToken);

            if (pref == null)
            {
                return new UserAlertPreferenceDto();
            }

            return new UserAlertPreferenceDto
            {
                EmailEnabled = pref.EmailEnabled,
                MonitorDownAlerts = pref.MonitorDownAlerts,
                CloudIncidentCriticalAlerts = pref.CloudIncidentCriticalAlerts,
                CloudIncidentMajorAlerts = pref.CloudIncidentMajorAlerts,
                SummaryEnabled = pref.SummaryEnabled,
                SummaryFrequency = pref.SummaryFrequency,
                SummaryDay = pref.SummaryDay,
                SummaryIncludeMonitors = pref.SummaryIncludeMonitors,
                SummaryIncludeCloud = pref.SummaryIncludeCloud,
                SelectedCloudProviderIds = pref.GetSelectedProviderIds(),
                AdditionalEmails = pref.GetAdditionalEmails(),
            };
        }

        public async Task UpdateMyPreferencesAsync(UserAlertPreferenceDto dto, CancellationToken cancellationToken = default)
        {
            var pref = await _dbContext.UserAlertPreferences
                .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, cancellationToken);

            if (pref == null)
            {
                pref = new UserAlertPreference
                {
                    Id = Guid.NewGuid(),
                    UserId = _currentUser.UserId,
                    TenantId = _currentUser.TenantId,
                    CreatedAt = DateTime.UtcNow,
                };
                _dbContext.UserAlertPreferences.Add(pref);
            }

            pref.EmailEnabled = dto.EmailEnabled;
            pref.MonitorDownAlerts = dto.MonitorDownAlerts;
            pref.CloudIncidentCriticalAlerts = dto.CloudIncidentCriticalAlerts;
            pref.CloudIncidentMajorAlerts = dto.CloudIncidentMajorAlerts;
            pref.SummaryEnabled = dto.SummaryEnabled;
            pref.SummaryFrequency = dto.SummaryFrequency;
            pref.SummaryDay = dto.SummaryDay;
            pref.SummaryIncludeMonitors = dto.SummaryIncludeMonitors;
            pref.SummaryIncludeCloud = dto.SummaryIncludeCloud;
            pref.SetSelectedProviderIds(dto.SelectedCloudProviderIds);
            pref.SetAdditionalEmails(dto.AdditionalEmails);
            pref.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task EnsureDefaultPreferencesAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            var exists = await _dbContext.UserAlertPreferences
                .AnyAsync(p => p.UserId == userId, cancellationToken);
            if (exists) return;

            _dbContext.UserAlertPreferences.Add(new UserAlertPreference
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TenantId = tenantId,
                EmailEnabled = true,
                MonitorDownAlerts = true,
                CloudIncidentCriticalAlerts = true,
                CloudIncidentMajorAlerts = true,
                SummaryEnabled = false,
                SummaryFrequency = SummaryFrequency.Weekly,
                SummaryDay = DayOfWeek.Sunday,
                SummaryIncludeMonitors = true,
                SummaryIncludeCloud = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
