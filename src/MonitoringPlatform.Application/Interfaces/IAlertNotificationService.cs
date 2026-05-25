using System;
using System.Threading;
using System.Threading.Tasks;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Application.Interfaces
{
    public interface IAlertNotificationService
    {
        Task NotifyMonitorDownAsync(Guid monitorId, string monitorName, string monitorUrl, string? errorMessage, CancellationToken cancellationToken = default);
        Task NotifyCloudIncidentAsync(string providerName, string incidentTitle, string incidentDescription, CloudIncidentSeverity severity, CancellationToken cancellationToken = default);
    }
}
