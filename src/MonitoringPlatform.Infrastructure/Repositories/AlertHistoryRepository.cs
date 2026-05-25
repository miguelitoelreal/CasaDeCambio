using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonitoringPlatform.Application.DTOs;
using MonitoringPlatform.Application.Interfaces;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Infrastructure.Persistence;

namespace MonitoringPlatform.Infrastructure.Repositories
{
    public class AlertHistoryRepository : IAlertHistoryRepository
    {
        private readonly AppDbContext _context;

        public AlertHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AlertHistoryResponseDto>> GetByTenantAsync(Guid tenantId, int limit = 50)
        {
            return await _context.AlertHistories
                .AsNoTracking()
                .Where(h => h.TenantId == tenantId)
                .OrderByDescending(h => h.SentAt)
                .Take(limit)
                .Select(h => ToDto(h))
                .ToListAsync();
        }

        public async Task<IEnumerable<AlertHistoryResponseDto>> GetRecentByTenantAndTypeAsync(Guid tenantId, AlertType alertType, TimeSpan window)
        {
            var since = DateTime.UtcNow.Subtract(window);
            return await _context.AlertHistories
                .AsNoTracking()
                .Where(h => h.TenantId == tenantId && h.AlertType == alertType && h.SentAt >= since && h.IsSuccess)
                .OrderByDescending(h => h.SentAt)
                .Select(h => ToDto(h))
                .ToListAsync();
        }

        public async Task RecordAsync(AlertHistory history)
        {
            _context.AlertHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        private static AlertHistoryResponseDto ToDto(AlertHistory h) => new()
        {
            Id = h.Id,
            TenantId = h.TenantId,
            AlertRuleId = h.AlertRuleId,
            AlertType = h.AlertType,
            AlertTypeLabel = AlertTypeLabel(h.AlertType),
            Channel = h.Channel,
            Subject = h.Subject,
            RecipientEmail = h.RecipientEmail,
            SentAt = h.SentAt,
            IsSuccess = h.IsSuccess,
            ErrorMessage = h.ErrorMessage,
        };

        private static string AlertTypeLabel(AlertType type) => type switch
        {
            AlertType.MonitorDown => "Monitor caído",
            AlertType.CloudIncidentCritical => "Incidencia crítica",
            AlertType.CloudIncidentMajor => "Incidencia mayor",
            _ => "Desconocido",
        };
    }
}
