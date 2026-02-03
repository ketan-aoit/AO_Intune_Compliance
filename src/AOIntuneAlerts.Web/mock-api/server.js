import http from 'http';

const PORT = 5001;

// Mock data
const mockDashboard = {
  complianceOverview: {
    totalDevices: 150,
    compliantDevices: 120,
    nonCompliantDevices: 20,
    approachingEosDevices: 8,
    unknownDevices: 2,
    compliancePercentage: 80,
  },
  recentAlerts: [
    { id: '1', subject: 'Windows 10 End of Support Warning', severity: 'Warning', sentAt: '2026-01-15T10:30:00Z' },
    { id: '2', subject: 'Non-compliant device detected', severity: 'Critical', sentAt: '2026-01-14T08:00:00Z' },
  ],
  devicesAtRisk: [
    { id: '1', deviceName: 'DESKTOP-ABC123', userDisplayName: 'John Smith', complianceState: 'NonCompliant', endOfSupportDate: '2026-03-15', issueCount: 3 },
    { id: '2', deviceName: 'LAPTOP-XYZ789', userDisplayName: 'Jane Doe', complianceState: 'ApproachingEndOfSupport', endOfSupportDate: '2026-04-01', issueCount: 1 },
  ],
  lastSyncTime: '2026-02-02T12:00:00Z',
  lastComplianceEvaluation: '2026-02-02T02:00:00Z',
};

const mockDevices = {
  items: [
    { id: '1', intuneDeviceId: 'intune-1', deviceName: 'DESKTOP-ABC123', userPrincipalName: 'john.smith@company.com', userDisplayName: 'John Smith', deviceType: 'Desktop', operatingSystem: 'Windows', osVersion: '10.0.19045', complianceState: 'NonCompliant', intuneComplianceState: 'Noncompliant', lastSyncDateTime: '2026-02-01T15:30:00Z', lastComplianceEvaluationDate: '2026-02-02T02:00:00Z', endOfSupportDate: '2026-03-15', isEncrypted: true, complianceIssueCount: 3 },
    { id: '2', intuneDeviceId: 'intune-2', deviceName: 'LAPTOP-XYZ789', userPrincipalName: 'jane.doe@company.com', userDisplayName: 'Jane Doe', deviceType: 'Laptop', operatingSystem: 'Windows', osVersion: '11.0.22631', complianceState: 'Compliant', intuneComplianceState: 'Compliant', lastSyncDateTime: '2026-02-02T10:00:00Z', lastComplianceEvaluationDate: '2026-02-02T02:00:00Z', endOfSupportDate: null, isEncrypted: true, complianceIssueCount: 0 },
    { id: '3', intuneDeviceId: 'intune-3', deviceName: 'MACBOOK-PRO-001', userPrincipalName: 'bob.wilson@company.com', userDisplayName: 'Bob Wilson', deviceType: 'Laptop', operatingSystem: 'macOS', osVersion: '14.2.1', complianceState: 'Compliant', intuneComplianceState: 'Compliant', lastSyncDateTime: '2026-02-02T09:00:00Z', lastComplianceEvaluationDate: '2026-02-02T02:00:00Z', endOfSupportDate: null, isEncrypted: true, complianceIssueCount: 0 },
    { id: '4', intuneDeviceId: 'intune-4', deviceName: 'DESKTOP-DEF456', userPrincipalName: 'alice.johnson@company.com', userDisplayName: 'Alice Johnson', deviceType: 'Desktop', operatingSystem: 'Windows', osVersion: '10.0.19044', complianceState: 'InGracePeriod', intuneComplianceState: 'InGracePeriod', lastSyncDateTime: '2026-02-02T08:00:00Z', lastComplianceEvaluationDate: null, endOfSupportDate: '2026-04-01', isEncrypted: true, complianceIssueCount: 1 },
  ],
  pageNumber: 1,
  totalPages: 1,
  totalCount: 4,
  hasPreviousPage: false,
  hasNextPage: false,
};

const mockDeviceDetail = {
  id: '1',
  intuneDeviceId: 'intune-1',
  deviceName: 'DESKTOP-ABC123',
  userPrincipalName: 'john.smith@company.com',
  userDisplayName: 'John Smith',
  deviceType: 'Desktop',
  operatingSystem: 'Windows',
  osVersion: '10.0.19045',
  osEdition: 'Enterprise',
  complianceState: 'NonCompliant',
  intuneComplianceState: 'Compliant',
  lastSyncDateTime: '2026-02-01T15:30:00Z',
  lastComplianceEvaluationDate: '2026-02-02T02:00:00Z',
  endOfSupportDate: '2026-03-15',
  isEncrypted: true,
  isManaged: true,
  serialNumber: 'SN123456789',
  model: 'OptiPlex 7090',
  manufacturer: 'Dell Inc.',
  createdAt: '2025-01-15T10:00:00Z',
  updatedAt: '2026-02-02T02:00:00Z',
  complianceIssueCount: 3,
  complianceIssues: [
    { id: '1', ruleId: 'rule-1', ruleName: 'OS Version Check', description: 'Windows 10 version is approaching end of support', severity: 'Warning', detectedAt: '2026-02-01T02:00:00Z' },
    { id: '2', ruleId: 'rule-2', ruleName: 'Browser Version Check', description: 'Chrome browser is outdated', severity: 'Medium', detectedAt: '2026-02-01T02:00:00Z' },
  ],
  browsers: [
    { id: '1', name: 'Chrome', version: '120.0.6099.130', isCompliant: false },
    { id: '2', name: 'Edge', version: '121.0.2277.83', isCompliant: true },
  ],
};

const mockComplianceRules = [
  // Patch Management - OS Versions
  { id: '1', name: 'Windows 10 Minimum Version', description: 'Cyber Essentials requires all Windows 10 devices to run a supported version. Minimum: 21H2 (Build 19044).', ruleType: 'OperatingSystemVersion', isEnabled: true, severity: 'Critical', configuration: '{"minVersion":"10.0.19044","endOfSupportWarningDays":90}', applicableOs: 'Windows', createdAt: '2025-01-01T00:00:00Z', updatedAt: null },
  { id: '2', name: 'Windows 11 Minimum Version', description: 'Cyber Essentials requires all Windows 11 devices to run a supported version. Minimum: 22H2.', ruleType: 'OperatingSystemVersion', isEnabled: true, severity: 'Critical', configuration: '{"minVersion":"10.0.22621"}', applicableOs: 'Windows', createdAt: '2025-01-01T00:00:00Z', updatedAt: null },
  { id: '3', name: 'macOS Minimum Version', description: 'Cyber Essentials requires macOS devices to run a supported version. Minimum: Ventura (13.0).', ruleType: 'OperatingSystemVersion', isEnabled: true, severity: 'Critical', configuration: '{"minVersion":"13.0.0"}', applicableOs: 'macOS', createdAt: '2025-01-01T00:00:00Z', updatedAt: null },
  { id: '4', name: 'iOS Minimum Version', description: 'Cyber Essentials requires iOS devices to run a supported version. Minimum: iOS 16.0.', ruleType: 'OperatingSystemVersion', isEnabled: true, severity: 'Critical', configuration: '{"minVersion":"16.0.0"}', applicableOs: 'iOS', createdAt: '2025-01-01T00:00:00Z', updatedAt: null },
  { id: '5', name: 'Android Minimum Version', description: 'Cyber Essentials requires Android devices to run Android 12+ with recent security patches.', ruleType: 'OperatingSystemVersion', isEnabled: true, severity: 'Critical', configuration: '{"minVersion":"12.0.0","maxSecurityPatchAgeDays":30}', applicableOs: 'Android', createdAt: '2025-01-01T00:00:00Z', updatedAt: null },
  // Patch Management - Browsers
  { id: '6', name: 'Microsoft Edge Minimum Version', description: 'Cyber Essentials requires browsers to be kept up to date. Edge should be within 2 major versions.', ruleType: 'BrowserVersion', isEnabled: true, severity: 'Warning', configuration: '{"browser":"Edge","minVersion":"120.0.0"}', applicableOs: null, createdAt: '2025-01-01T00:00:00Z', updatedAt: null },
  { id: '7', name: 'Google Chrome Minimum Version', description: 'Cyber Essentials requires browsers to be kept up to date. Chrome should be within 2 major versions.', ruleType: 'BrowserVersion', isEnabled: true, severity: 'Warning', configuration: '{"browser":"Chrome","minVersion":"120.0.0"}', applicableOs: null, createdAt: '2025-01-01T00:00:00Z', updatedAt: null },
  { id: '8', name: 'Mozilla Firefox Minimum Version', description: 'Cyber Essentials requires browsers to be kept up to date. Firefox should be within 2 major versions.', ruleType: 'BrowserVersion', isEnabled: true, severity: 'Warning', configuration: '{"browser":"Firefox","minVersion":"120.0.0"}', applicableOs: null, createdAt: '2025-01-01T00:00:00Z', updatedAt: null },
  // Security Software
  { id: '9', name: 'Windows Defender Antivirus', description: 'Cyber Essentials requires anti-malware with real-time protection and up-to-date definitions.', ruleType: 'SecuritySoftware', isEnabled: true, severity: 'Critical', configuration: '{"requireRealTimeProtection":true,"maxDefinitionAgeDays":7}', applicableOs: 'Windows', createdAt: '2025-01-01T00:00:00Z', updatedAt: null },
  { id: '10', name: 'macOS Gatekeeper and XProtect', description: 'Cyber Essentials requires malware protection. Gatekeeper must be enabled.', ruleType: 'SecuritySoftware', isEnabled: true, severity: 'Critical', configuration: '{"requireGatekeeper":true}', applicableOs: 'macOS', createdAt: '2025-01-01T00:00:00Z', updatedAt: null },
  // Encryption
  { id: '11', name: 'Windows BitLocker Encryption', description: 'Cyber Essentials requires device encryption. BitLocker must be enabled.', ruleType: 'EncryptionEnabled', isEnabled: true, severity: 'Critical', configuration: '{"requireSystemDriveEncryption":true}', applicableOs: 'Windows', createdAt: '2025-01-01T00:00:00Z', updatedAt: null },
  { id: '12', name: 'macOS FileVault Encryption', description: 'Cyber Essentials requires device encryption. FileVault 2 must be enabled.', ruleType: 'EncryptionEnabled', isEnabled: true, severity: 'Critical', configuration: '{"requireFileVault":true}', applicableOs: 'macOS', createdAt: '2025-01-01T00:00:00Z', updatedAt: null },
  { id: '13', name: 'Mobile Device Encryption', description: 'Cyber Essentials requires mobile device encryption with passcode protection.', ruleType: 'EncryptionEnabled', isEnabled: true, severity: 'Critical', configuration: '{"requireEncryption":true,"requirePasscode":true}', applicableOs: null, createdAt: '2025-01-01T00:00:00Z', updatedAt: null },
  // Firewall
  { id: '14', name: 'Windows Firewall Enabled', description: 'Cyber Essentials requires host-based firewall on all Windows devices.', ruleType: 'FirewallEnabled', isEnabled: true, severity: 'Critical', configuration: '{"requireDomainProfile":true,"requirePrivateProfile":true,"requirePublicProfile":true}', applicableOs: 'Windows', createdAt: '2025-01-01T00:00:00Z', updatedAt: null },
  { id: '15', name: 'macOS Firewall Enabled', description: 'Cyber Essentials requires host-based firewall. macOS Application Firewall must be enabled.', ruleType: 'FirewallEnabled', isEnabled: true, severity: 'Critical', configuration: '{"requireFirewall":true}', applicableOs: 'macOS', createdAt: '2025-01-01T00:00:00Z', updatedAt: null },
];

const mockAlertRecipients = [
  { id: '1', email: 'admin@company.com', displayName: 'IT Admin', isEnabled: true, minimumSeverity: 'Warning', createdAt: '2025-01-01T00:00:00Z' },
  { id: '2', email: 'security@company.com', displayName: 'Security Team', isEnabled: true, minimumSeverity: 'Critical', createdAt: '2025-01-01T00:00:00Z' },
];

const mockAlertHistory = {
  items: [
    { id: '1', deviceId: '1', subject: 'Windows 10 End of Support Warning', severity: 'Warning', createdAt: '2026-01-15T10:30:00Z', sentAt: '2026-01-15T10:30:00Z', wasSent: true, errorMessage: null, recipientCount: 2 },
    { id: '2', deviceId: '1', subject: 'Non-compliant device detected', severity: 'Critical', createdAt: '2026-01-14T08:00:00Z', sentAt: '2026-01-14T08:00:00Z', wasSent: true, errorMessage: null, recipientCount: 2 },
  ],
  pageNumber: 1,
  totalPages: 1,
  totalCount: 2,
  hasPreviousPage: false,
  hasNextPage: false,
};

// Request handler
const server = http.createServer((req, res) => {
  // CORS headers
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization, X-Dev-Auth, X-Dev-User, X-Dev-Role');
  res.setHeader('Content-Type', 'application/json');

  // Handle preflight
  if (req.method === 'OPTIONS') {
    res.writeHead(204);
    res.end();
    return;
  }

  const url = new URL(req.url, `http://localhost:${PORT}`);
  const path = url.pathname;

  console.log(`[Mock API] ${req.method} ${path}`);

  // Check for dev auth header
  const devAuth = req.headers['x-dev-auth'];
  if (!devAuth || devAuth !== 'dev-test-secret-12345') {
    res.writeHead(401);
    res.end(JSON.stringify({ error: 'Unauthorized' }));
    return;
  }

  // Route handling
  if (path === '/api/v1/dashboard' && req.method === 'GET') {
    res.writeHead(200);
    res.end(JSON.stringify(mockDashboard));
  } else if (path === '/api/v1/devices' && req.method === 'GET') {
    res.writeHead(200);
    res.end(JSON.stringify(mockDevices));
  } else if (path.match(/^\/api\/v1\/devices\/[^/]+$/) && req.method === 'GET') {
    res.writeHead(200);
    res.end(JSON.stringify(mockDeviceDetail));
  } else if (path === '/api/v1/devices/sync' && req.method === 'POST') {
    res.writeHead(200);
    res.end(JSON.stringify({ devicesSynced: 150, devicesCreated: 0, devicesUpdated: 5 }));
  } else if (path === '/api/v1/compliance-rules' && req.method === 'GET') {
    res.writeHead(200);
    res.end(JSON.stringify(mockComplianceRules));
  } else if (path === '/api/v1/alerts/recipients' && req.method === 'GET') {
    res.writeHead(200);
    res.end(JSON.stringify(mockAlertRecipients));
  } else if (path === '/api/v1/alerts/history' && req.method === 'GET') {
    res.writeHead(200);
    res.end(JSON.stringify(mockAlertHistory));
  } else if (path === '/health' && req.method === 'GET') {
    res.writeHead(200);
    res.end(JSON.stringify({ status: 'Healthy' }));
  } else {
    res.writeHead(404);
    res.end(JSON.stringify({ error: 'Not found' }));
  }
});

server.listen(PORT, () => {
  console.log(`[Mock API] Server running at http://localhost:${PORT}`);
  console.log('[Mock API] Dev auth secret: dev-test-secret-12345');
});
