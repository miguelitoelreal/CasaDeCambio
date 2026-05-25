export const AlertType = {
  MonitorDown: 1,
  CloudIncidentCritical: 2,
  CloudIncidentMajor: 3,
} as const;

export type AlertType = (typeof AlertType)[keyof typeof AlertType];

export const AlertChannel = {
  Email: 1,
} as const;

export type AlertChannel = (typeof AlertChannel)[keyof typeof AlertChannel];

export interface AlertRule {
  id: string;
  tenantId: string;
  name: string;
  alertType: AlertType;
  alertTypeLabel: string;
  channel: AlertChannel;
  channelLabel: string;
  isEnabled: boolean;
  throttleMinutes: number;
  recipientEmails: string[];
  selectedCloudProviderIds: string[];
  createdAt: string;
  updatedAt: string;
}

export interface AlertHistory {
  id: string;
  tenantId: string;
  alertRuleId?: string;
  alertType: AlertType;
  alertTypeLabel: string;
  channel: AlertChannel;
  subject: string;
  recipientEmail: string;
  sentAt: string;
  isSuccess: boolean;
  errorMessage?: string;
}

export interface CreateAlertRulePayload {
  name: string;
  alertType: AlertType;
  channel: AlertChannel;
  throttleMinutes: number;
  recipientEmails: string[];
  selectedCloudProviderIds: string[];
}

export interface UpdateAlertRulePayload {
  name: string;
  alertType: AlertType;
  channel: AlertChannel;
  isEnabled: boolean;
  throttleMinutes: number;
  recipientEmails: string[];
  selectedCloudProviderIds: string[];
}

export const SummaryFrequency = {
  Daily: 1,
  Weekly: 2,
  Monthly: 3,
} as const;

export type SummaryFrequency = (typeof SummaryFrequency)[keyof typeof SummaryFrequency];

export interface UserAlertPreference {
  emailEnabled: boolean;
  monitorDownAlerts: boolean;
  cloudIncidentCriticalAlerts: boolean;
  cloudIncidentMajorAlerts: boolean;
  summaryEnabled: boolean;
  summaryFrequency: SummaryFrequency;
  summaryDay: number;
  summaryIncludeMonitors: boolean;
  summaryIncludeCloud: boolean;
  selectedCloudProviderIds: string[];
  additionalEmails: string[];
}

export interface CloudProviderOption {
  id: string;
  name: string;
}

export function alertTypeLabel(type: AlertType): string {
  switch (type) {
    case AlertType.MonitorDown:
      return "Monitor caído";
    case AlertType.CloudIncidentCritical:
      return "Incidencia crítica";
    case AlertType.CloudIncidentMajor:
      return "Incidencia mayor";
    default:
      return "Desconocido";
  }
}

export function alertChannelLabel(channel: AlertChannel): string {
  switch (channel) {
    case AlertChannel.Email:
      return "Email";
    default:
      return "Desconocido";
  }
}
