export interface PaginatedList<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Compliance states from Intune
export type ComplianceState =
  | 'Unknown'
  | 'Compliant'
  | 'NonCompliant'
  | 'ApproachingEndOfSupport'
  | 'InGracePeriod'
  | 'ConfigManager'
  | 'Conflict'
  | 'Error';

export interface DeviceDto {
  id: string;
  intuneDeviceId: string;
  deviceName: string;
  userPrincipalName: string | null;
  userDisplayName: string | null;
  deviceType: string;
  operatingSystem: string;
  osVersion: string;
  complianceState: ComplianceState;
  intuneComplianceState: ComplianceState;
  lastSyncDateTime: string | null;
  lastComplianceEvaluationDate: string | null;
  endOfSupportDate: string | null;
  isEncrypted: boolean;
  complianceIssueCount: number;
}

export interface DeviceDetailDto extends DeviceDto {
  osEdition: string | null;
  isManaged: boolean;
  serialNumber: string | null;
  model: string | null;
  manufacturer: string | null;
  createdAt: string;
  updatedAt: string | null;
  complianceIssues: ComplianceIssueDto[];
  browsers: BrowserDto[];
}

export interface ComplianceIssueDto {
  id: string;
  ruleId: string;
  ruleName: string;
  description: string;
  severity: string;
  detectedAt: string;
}

export interface BrowserDto {
  id: string;
  name: string;
  version: string;
  isCompliant: boolean;
}

export interface ComplianceSummaryDto {
  totalDevices: number;
  compliantDevices: number;
  nonCompliantDevices: number;
  approachingEosDevices: number;
  unknownDevices: number;
  compliancePercentage: number;
  osDistribution: OsDistributionDto[];
  topComplianceIssues: ComplianceIssueCountDto[];
}

export interface OsDistributionDto {
  osName: string;
  count: number;
  percentage: number;
}

export interface ComplianceIssueCountDto {
  ruleName: string;
  deviceCount: number;
}

export interface DashboardDto {
  complianceOverview: ComplianceOverviewDto;
  recentAlerts: RecentAlertDto[];
  devicesAtRisk: DeviceAtRiskDto[];
  lastSyncTime: string | null;
  lastComplianceEvaluation: string | null;
}

export interface ComplianceOverviewDto {
  totalDevices: number;
  compliantDevices: number;
  nonCompliantDevices: number;
  approachingEosDevices: number;
  unknownDevices: number;
  compliancePercentage: number;
}

export interface RecentAlertDto {
  id: string;
  subject: string;
  severity: string;
  sentAt: string;
}

export interface DeviceAtRiskDto {
  id: string;
  deviceName: string;
  userDisplayName: string | null;
  complianceState: string;
  endOfSupportDate: string | null;
  issueCount: number;
}

export interface ComplianceRuleDto {
  id: string;
  name: string;
  description: string;
  ruleType: string;
  isEnabled: boolean;
  severity: string;
  configuration: string;
  applicableOs: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface AlertRecipientDto {
  id: string;
  email: string;
  displayName: string;
  isEnabled: boolean;
  minimumSeverity: string;
  createdAt: string;
}

export interface AlertHistoryDto {
  id: string;
  deviceId: string | null;
  subject: string;
  severity: string;
  createdAt: string;
  sentAt: string | null;
  wasSent: boolean;
  errorMessage: string | null;
  recipientCount: number;
}

export interface SyncDevicesResult {
  devicesSynced: number;
  devicesCreated: number;
  devicesUpdated: number;
}
