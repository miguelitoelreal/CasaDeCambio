using System;
using System.Collections.Generic;

namespace MonitoringPlatform.Domain.Entities
{
    public enum SummaryFrequency
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
    }

    public class UserAlertPreference
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }

        public bool EmailEnabled { get; set; } = true;

        public bool MonitorDownAlerts { get; set; } = true;
        public bool CloudIncidentCriticalAlerts { get; set; } = true;
        public bool CloudIncidentMajorAlerts { get; set; } = true;

        public bool SummaryEnabled { get; set; } = false;
        public SummaryFrequency SummaryFrequency { get; set; } = SummaryFrequency.Weekly;
        public DayOfWeek SummaryDay { get; set; } = DayOfWeek.Sunday;
        public bool SummaryIncludeMonitors { get; set; } = true;
        public bool SummaryIncludeCloud { get; set; } = true;

        public string SelectedCloudProviderIds { get; set; } = string.Empty;
        public string AdditionalEmails { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Tenant Tenant { get; set; } = null!;

        public List<Guid> GetSelectedProviderIds()
        {
            if (string.IsNullOrWhiteSpace(SelectedCloudProviderIds)) return new List<Guid>();
            var ids = new List<Guid>();
            foreach (var part in SelectedCloudProviderIds.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (Guid.TryParse(part.Trim(), out var id)) ids.Add(id);
            }
            return ids;
        }

        public void SetSelectedProviderIds(IEnumerable<Guid> ids)
        {
            SelectedCloudProviderIds = string.Join(",", ids);
        }

        public List<string> GetAdditionalEmails()
        {
            if (string.IsNullOrWhiteSpace(AdditionalEmails)) return new List<string>();
            var emails = new List<string>();
            foreach (var part in AdditionalEmails.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = part.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed)) emails.Add(trimmed);
            }
            return emails;
        }

        public void SetAdditionalEmails(IEnumerable<string> emails)
        {
            AdditionalEmails = string.Join(",", emails);
        }
    }
}
