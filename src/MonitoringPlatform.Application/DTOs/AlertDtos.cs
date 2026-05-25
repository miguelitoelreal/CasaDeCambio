using System;
using MonitoringPlatform.Domain.Entities;

namespace MonitoringPlatform.Application.DTOs
{
    public class CreateAlertRuleDto
    {
        public string Name { get; set; } = null!;
        public AlertType AlertType { get; set; }
        public AlertChannel Channel { get; set; }
        public int ThrottleMinutes { get; set; } = 15;
        public List<string> RecipientEmails { get; set; } = new();
        public List<Guid> SelectedCloudProviderIds { get; set; } = new();
    }

    public class UpdateAlertRuleDto
    {
        public string Name { get; set; } = null!;
        public AlertType AlertType { get; set; }
        public AlertChannel Channel { get; set; }
        public bool IsEnabled { get; set; }
        public int ThrottleMinutes { get; set; } = 15;
        public List<string> RecipientEmails { get; set; } = new();
        public List<Guid> SelectedCloudProviderIds { get; set; } = new();
    }

    public class AlertRuleResponseDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Name { get; set; } = null!;
        public AlertType AlertType { get; set; }
        public string AlertTypeLabel { get; set; } = null!;
        public AlertChannel Channel { get; set; }
        public string ChannelLabel { get; set; } = null!;
        public bool IsEnabled { get; set; }
        public int ThrottleMinutes { get; set; }
        public List<string> RecipientEmails { get; set; } = new();
        public List<Guid> SelectedCloudProviderIds { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UserAlertPreferenceDto
    {
        public bool EmailEnabled { get; set; }
        public bool MonitorDownAlerts { get; set; }
        public bool CloudIncidentCriticalAlerts { get; set; }
        public bool CloudIncidentMajorAlerts { get; set; }
        public bool SummaryEnabled { get; set; }
        public SummaryFrequency SummaryFrequency { get; set; }
        public DayOfWeek SummaryDay { get; set; }
        public bool SummaryIncludeMonitors { get; set; }
        public bool SummaryIncludeCloud { get; set; }
        public List<Guid> SelectedCloudProviderIds { get; set; } = new();
        public List<string> AdditionalEmails { get; set; } = new();
    }

    public class TestAlertRequestDto
    {
        public string Type { get; set; } = "monitor";
    }

    public class CloudProviderOptionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class AlertHistoryResponseDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid? AlertRuleId { get; set; }
        public AlertType AlertType { get; set; }
        public string AlertTypeLabel { get; set; } = null!;
        public AlertChannel Channel { get; set; }
        public string Subject { get; set; } = null!;
        public string RecipientEmail { get; set; } = null!;
        public DateTime SentAt { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
