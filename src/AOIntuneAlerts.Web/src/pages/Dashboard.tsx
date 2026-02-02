import {
  makeStyles,
  tokens,
  Card,
  CardHeader,
  Text,
  Title3,
  Badge,
  Spinner,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
} from '@fluentui/react-components';
import {
  CheckmarkCircle20Filled,
  DismissCircle20Filled,
  Warning20Filled,
} from '@fluentui/react-icons';
import { useDashboard } from '../api';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    gap: tokens.spacingVerticalL,
  },
  statsGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
    gap: tokens.spacingHorizontalM,
  },
  statCard: {
    padding: tokens.spacingVerticalM,
  },
  statValue: {
    fontSize: '32px',
    fontWeight: 600,
    marginTop: tokens.spacingVerticalS,
  },
  statLabel: {
    color: tokens.colorNeutralForeground3,
  },
  grid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(400px, 1fr))',
    gap: tokens.spacingHorizontalL,
  },
  compliant: {
    color: tokens.colorPaletteGreenForeground1,
  },
  nonCompliant: {
    color: tokens.colorPaletteRedForeground1,
  },
  warning: {
    color: tokens.colorPaletteYellowForeground1,
  },
  unknown: {
    color: tokens.colorNeutralForeground3,
  },
  compliancePercentage: {
    fontSize: '48px',
    fontWeight: 600,
  },
});

export function Dashboard() {
  const styles = useStyles();
  const { data: dashboard, isLoading } = useDashboard();

  if (isLoading) {
    return <Spinner size="large" label="Loading dashboard..." />;
  }

  if (!dashboard) {
    return <Text>Failed to load dashboard</Text>;
  }

  const { complianceOverview, recentAlerts, devicesAtRisk } = dashboard;

  return (
    <div className={styles.container}>
      <div className={styles.statsGrid}>
        <Card className={styles.statCard}>
          <Text className={styles.statLabel}>Total Devices</Text>
          <Text className={styles.statValue}>{complianceOverview.totalDevices}</Text>
        </Card>
        <Card className={styles.statCard}>
          <Text className={styles.statLabel}>
            <CheckmarkCircle20Filled className={styles.compliant} /> Compliant
          </Text>
          <Text className={`${styles.statValue} ${styles.compliant}`}>
            {complianceOverview.compliantDevices}
          </Text>
        </Card>
        <Card className={styles.statCard}>
          <Text className={styles.statLabel}>
            <DismissCircle20Filled className={styles.nonCompliant} /> Non-Compliant
          </Text>
          <Text className={`${styles.statValue} ${styles.nonCompliant}`}>
            {complianceOverview.nonCompliantDevices}
          </Text>
        </Card>
        <Card className={styles.statCard}>
          <Text className={styles.statLabel}>
            <Warning20Filled className={styles.warning} /> Approaching EOS
          </Text>
          <Text className={`${styles.statValue} ${styles.warning}`}>
            {complianceOverview.approachingEosDevices}
          </Text>
        </Card>
        <Card className={styles.statCard}>
          <Text className={styles.statLabel}>Compliance Rate</Text>
          <Text className={`${styles.compliancePercentage} ${styles.compliant}`}>
            {complianceOverview.compliancePercentage}%
          </Text>
        </Card>
      </div>

      <div className={styles.grid}>
        <Card>
          <CardHeader header={<Title3>Devices at Risk</Title3>} />
          <Table>
            <TableHeader>
              <TableRow>
                <TableHeaderCell>Device</TableHeaderCell>
                <TableHeaderCell>User</TableHeaderCell>
                <TableHeaderCell>Status</TableHeaderCell>
                <TableHeaderCell>Issues</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {devicesAtRisk.slice(0, 5).map((device) => (
                <TableRow key={device.id}>
                  <TableCell>{device.deviceName}</TableCell>
                  <TableCell>{device.userDisplayName || '-'}</TableCell>
                  <TableCell>
                    <ComplianceBadge state={device.complianceState} />
                  </TableCell>
                  <TableCell>{device.issueCount}</TableCell>
                </TableRow>
              ))}
              {devicesAtRisk.length === 0 && (
                <TableRow>
                  <TableCell colSpan={4}>
                    <Text>No devices at risk</Text>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </Card>

        <Card>
          <CardHeader header={<Title3>Recent Alerts</Title3>} />
          <Table>
            <TableHeader>
              <TableRow>
                <TableHeaderCell>Subject</TableHeaderCell>
                <TableHeaderCell>Severity</TableHeaderCell>
                <TableHeaderCell>Sent</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {recentAlerts.slice(0, 5).map((alert) => (
                <TableRow key={alert.id}>
                  <TableCell>{alert.subject}</TableCell>
                  <TableCell>
                    <SeverityBadge severity={alert.severity} />
                  </TableCell>
                  <TableCell>{new Date(alert.sentAt).toLocaleDateString()}</TableCell>
                </TableRow>
              ))}
              {recentAlerts.length === 0 && (
                <TableRow>
                  <TableCell colSpan={3}>
                    <Text>No recent alerts</Text>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </Card>
      </div>

      <Text size={200} style={{ color: tokens.colorNeutralForeground3 }}>
        Last sync: {dashboard.lastSyncTime ? new Date(dashboard.lastSyncTime).toLocaleString() : 'Never'} |
        Last evaluation: {dashboard.lastComplianceEvaluation ? new Date(dashboard.lastComplianceEvaluation).toLocaleString() : 'Never'}
      </Text>
    </div>
  );
}

function ComplianceBadge({ state }: { state: string }) {
  const color =
    state === 'Compliant'
      ? 'success'
      : state === 'NonCompliant'
      ? 'danger'
      : state === 'ApproachingEndOfSupport'
      ? 'warning'
      : 'informative';

  return <Badge appearance="filled" color={color}>{state}</Badge>;
}

function SeverityBadge({ severity }: { severity: string }) {
  const color =
    severity === 'Critical'
      ? 'danger'
      : severity === 'Warning'
      ? 'warning'
      : 'informative';

  return <Badge appearance="filled" color={color}>{severity}</Badge>;
}
