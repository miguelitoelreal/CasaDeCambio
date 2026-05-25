using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MonitoringPlatform.Application.DTOs;
using MonitoringPlatform.Application.Interfaces;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Enums;
using MonitoringPlatform.Infrastructure.Persistence;
using MonitoringPlatform.Infrastructure.Persistence.Identity;

namespace MonitoringPlatform.API.Services
{
    public class AlertNotificationService : IAlertNotificationService
    {
        private readonly AppDbContext _context;
        private readonly IAlertRuleRepository _ruleRepository;
        private readonly IAlertHistoryRepository _historyRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<AlertNotificationService> _logger;

        public AlertNotificationService(
            AppDbContext context,
            IAlertRuleRepository ruleRepository,
            IAlertHistoryRepository historyRepository,
            IEmailService emailService,
            ILogger<AlertNotificationService> logger)
        {
            _context = context;
            _ruleRepository = ruleRepository;
            _historyRepository = historyRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task NotifyMonitorDownAsync(Guid monitorId, string monitorName, string monitorUrl, string? errorMessage, CancellationToken cancellationToken = default)
        {
            var monitor = await _context.Monitors
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == monitorId, cancellationToken);

            if (monitor == null)
            {
                _logger.LogWarning("Monitor {MonitorId} not found for alert", monitorId);
                return;
            }

            var tenantId = monitor.TenantId;
            var rules = (await _ruleRepository.GetEnabledByTenantAndTypeAsync(tenantId, AlertType.MonitorDown)).ToList();
            if (!rules.Any()) return;

            var firstRule = rules.First();
            var throttle = TimeSpan.FromMinutes(firstRule.ThrottleMinutes > 0 ? firstRule.ThrottleMinutes : 15);
            var recentAlerts = await _historyRepository.GetRecentByTenantAndTypeAsync(tenantId, AlertType.MonitorDown, throttle);
            if (recentAlerts.Any())
            {
                _logger.LogInformation("Throttling monitor down alert for tenant {TenantId}", tenantId);
                return;
            }

            var allEmails = await GetRecipientEmailsAsync(tenantId, AlertType.MonitorDown, firstRule.RecipientEmails, cancellationToken);
            if (!allEmails.Any()) return;

            var subject = $"🚨 Alerta: Monitor caído – {monitorName}";
            var body = $@"
                <h2>Cloud Alert Hub – Alerta de Monitor</h2>
                <p>El monitor <strong>{monitorName}</strong> ha cambiado a estado <strong>Offline</strong>.</p>
                <ul>
                    <li><strong>URL:</strong> {monitorUrl}</li>
                    <li><strong>Error:</strong> {errorMessage ?? "Sin detalle"}</li>
                    <li><strong>Hora:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</li>
                </ul>
                <p>Accede al <a href='https://localhost:5173/dashboard'>Centro de Monitoreo</a> para más detalles.</p>
            ";

            foreach (var email in allEmails)
            {
                var success = await _emailService.SendEmailAsync(email, subject, body);
                await _historyRepository.RecordAsync(new AlertHistory
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    AlertRuleId = firstRule.Id,
                    AlertType = AlertType.MonitorDown,
                    Channel = AlertChannel.Email,
                    Subject = subject,
                    Message = body,
                    RecipientEmail = email,
                    SentAt = DateTime.UtcNow,
                    IsSuccess = success,
                    ErrorMessage = success ? null : "Falló el envío de email",
                });
            }
        }

        public async Task NotifyCloudIncidentAsync(string providerName, string incidentTitle, string incidentDescription, CloudIncidentSeverity severity, CancellationToken cancellationToken = default)
        {
            var alertType = severity == CloudIncidentSeverity.Critical
                ? AlertType.CloudIncidentCritical
                : AlertType.CloudIncidentMajor;

            var rules = (await _ruleRepository.GetAllEnabledByTypeAsync(alertType)).ToList();
            if (!rules.Any()) return;

            foreach (var rule in rules)
            {
                var tenantId = rule.TenantId;
                var throttle = TimeSpan.FromMinutes(rule.ThrottleMinutes > 0 ? rule.ThrottleMinutes : 15);
                var recentAlerts = await _historyRepository.GetRecentByTenantAndTypeAsync(tenantId, alertType, throttle);
                if (recentAlerts.Any())
                {
                    _logger.LogInformation("Throttling cloud incident alert for tenant {TenantId}", tenantId);
                    continue;
                }

                var allEmails = await GetRecipientEmailsAsync(tenantId, alertType, rule.RecipientEmails, cancellationToken);
                if (!allEmails.Any()) continue;

                var subject = $"☁️ Alerta Cloud – {providerName}: {incidentTitle}";
                var body = $@"
                    <h2>Cloud Alert Hub – Incidencia Cloud</h2>
                    <p><strong>Proveedor:</strong> {providerName}</p>
                    <p><strong>Título:</strong> {incidentTitle}</p>
                    <p><strong>Severidad:</strong> {severity}</p>
                    <p><strong>Descripción:</strong></p>
                    <blockquote>{incidentDescription}</blockquote>
                    <p><strong>Hora:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                    <p>Accede al <a href='https://localhost:5173/centro-estado-cloud'>Centro de Estado Cloud</a> para más detalles.</p>
                ";

                foreach (var email in allEmails)
                {
                    var success = await _emailService.SendEmailAsync(email, subject, body);
                    await _historyRepository.RecordAsync(new AlertHistory
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        AlertRuleId = rule.Id,
                        AlertType = alertType,
                        Channel = AlertChannel.Email,
                        Subject = subject,
                        Message = body,
                        RecipientEmail = email,
                        SentAt = DateTime.UtcNow,
                        IsSuccess = success,
                        ErrorMessage = success ? null : "Falló el envío de email",
                    });
                }
            }
        }

        private async Task<List<string>> GetRecipientEmailsAsync(Guid tenantId, AlertType alertType, List<string> ruleExtraEmails, CancellationToken cancellationToken)
        {
            var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var users = await _context.Users
                .AsNoTracking()
                .Where(u => u.TenantId == tenantId && u.EmailConfirmed)
                .ToListAsync(cancellationToken);

            var userIds = users.Select(u => u.Id).ToList();
            var preferences = await _context.UserAlertPreferences
                .AsNoTracking()
                .Where(p => userIds.Contains(p.UserId))
                .ToListAsync(cancellationToken);

            var prefMap = preferences.ToDictionary(p => p.UserId);

            foreach (var user in users)
            {
                if (string.IsNullOrWhiteSpace(user.Email)) continue;

                if (!prefMap.TryGetValue(user.Id, out var pref))
                {
                    emails.Add(user.Email);
                    continue;
                }

                if (!pref.EmailEnabled) continue;

                var shouldSend = alertType switch
                {
                    AlertType.MonitorDown => pref.MonitorDownAlerts,
                    AlertType.CloudIncidentCritical => pref.CloudIncidentCriticalAlerts,
                    AlertType.CloudIncidentMajor => pref.CloudIncidentMajorAlerts,
                    _ => true,
                };

                if (shouldSend)
                {
                    emails.Add(user.Email);
                }

                foreach (var extra in pref.GetAdditionalEmails())
                {
                    if (!string.IsNullOrWhiteSpace(extra)) emails.Add(extra);
                }
            }

            foreach (var extra in ruleExtraEmails)
            {
                if (!string.IsNullOrWhiteSpace(extra)) emails.Add(extra);
            }

            return emails.ToList();
        }
    }
}
