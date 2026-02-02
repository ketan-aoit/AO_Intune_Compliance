import { useParams, useNavigate } from 'react-router-dom';
import {
  makeStyles,
  tokens,
  Card,
  CardHeader,
  Text,
  Title2,
  Title3,
  Button,
  Badge,
  Spinner,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
} from '@fluentui/react-components';
import { ArrowLeft20Regular, ArrowSync20Regular } from '@fluentui/react-icons';
import { useDevice, useEvaluateCompliance } from '../api';
import { useAuth } from '../auth';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    gap: tokens.spacingVerticalL,
  },
  header: {
    display: 'flex',
    alignItems: 'center',
    gap: tokens.spacingHorizontalM,
  },
  detailsGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))',
    gap: tokens.spacingHorizontalL,
  },
  detailSection: {
    display: 'flex',
    flexDirection: 'column',
    gap: tokens.spacingVerticalS,
  },
  detailRow: {
    display: 'flex',
    justifyContent: 'space-between',
    padding: `${tokens.spacingVerticalXS} 0`,
  },
  detailLabel: {
    color: tokens.colorNeutralForeground3,
  },
});

export function DeviceDetail() {
  const styles = useStyles();
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { isAdmin } = useAuth();
  const { data: device, isLoading } = useDevice(id || '');
  const evaluateMutation = useEvaluateCompliance();

  if (isLoading) {
    return <Spinner size="large" label="Loading device..." />;
  }

  if (!device) {
    return <Text>Device not found</Text>;
  }

  const handleEvaluate = async () => {
    if (id) {
      await evaluateMutation.mutateAsync(id);
    }
  };

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <Button icon={<ArrowLeft20Regular />} appearance="subtle" onClick={() => navigate('/devices')} />
        <Title2>{device.deviceName}</Title2>
        <ComplianceBadge state={device.complianceState} />
        {isAdmin() && (
          <Button
            icon={<ArrowSync20Regular />}
            onClick={handleEvaluate}
            disabled={evaluateMutation.isPending}
          >
            {evaluateMutation.isPending ? 'Evaluating...' : 'Evaluate Compliance'}
          </Button>
        )}
      </div>

      <div className={styles.detailsGrid}>
        <Card>
          <CardHeader header={<Title3>Device Information</Title3>} />
          <div className={styles.detailSection}>
            <DetailRow label="Intune Device ID" value={device.intuneDeviceId} />
            <DetailRow label="Device Type" value={device.deviceType} />
            <DetailRow label="Serial Number" value={device.serialNumber || '-'} />
            <DetailRow label="Model" value={device.model || '-'} />
            <DetailRow label="Manufacturer" value={device.manufacturer || '-'} />
            <DetailRow label="Encrypted" value={device.isEncrypted ? 'Yes' : 'No'} />
            <DetailRow label="Managed" value={device.isManaged ? 'Yes' : 'No'} />
          </div>
        </Card>

        <Card>
          <CardHeader header={<Title3>Operating System</Title3>} />
          <div className={styles.detailSection}>
            <DetailRow label="OS" value={device.operatingSystem} />
            <DetailRow label="Version" value={device.osVersion} />
            <DetailRow label="Edition" value={device.osEdition || '-'} />
            <DetailRow
              label="End of Support"
              value={device.endOfSupportDate ? new Date(device.endOfSupportDate).toLocaleDateString() : 'N/A'}
            />
          </div>
        </Card>

        <Card>
          <CardHeader header={<Title3>User Assignment</Title3>} />
          <div className={styles.detailSection}>
            <DetailRow label="User" value={device.userDisplayName || '-'} />
            <DetailRow label="UPN" value={device.userPrincipalName || '-'} />
          </div>
        </Card>

        <Card>
          <CardHeader header={<Title3>Compliance Status</Title3>} />
          <div className={styles.detailSection}>
            <DetailRow label="Portal Status" value={device.complianceState} />
            <DetailRow label="Intune Status" value={device.intuneComplianceState} />
            <DetailRow
              label="Last Sync"
              value={device.lastSyncDateTime ? new Date(device.lastSyncDateTime).toLocaleString() : '-'}
            />
            <DetailRow
              label="Last Evaluation"
              value={device.lastComplianceEvaluationDate ? new Date(device.lastComplianceEvaluationDate).toLocaleString() : '-'}
            />
          </div>
        </Card>
      </div>

      {device.complianceIssues.length > 0 && (
        <Card>
          <CardHeader header={<Title3>Compliance Issues ({device.complianceIssues.length})</Title3>} />
          <Table>
            <TableHeader>
              <TableRow>
                <TableHeaderCell>Issue</TableHeaderCell>
                <TableHeaderCell>Description</TableHeaderCell>
                <TableHeaderCell>Severity</TableHeaderCell>
                <TableHeaderCell>Detected</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {device.complianceIssues.map((issue) => (
                <TableRow key={issue.id}>
                  <TableCell>{issue.ruleName}</TableCell>
                  <TableCell>{issue.description}</TableCell>
                  <TableCell>
                    <SeverityBadge severity={issue.severity} />
                  </TableCell>
                  <TableCell>{new Date(issue.detectedAt).toLocaleDateString()}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Card>
      )}

      {device.browsers.length > 0 && (
        <Card>
          <CardHeader header={<Title3>Installed Browsers</Title3>} />
          <Table>
            <TableHeader>
              <TableRow>
                <TableHeaderCell>Browser</TableHeaderCell>
                <TableHeaderCell>Version</TableHeaderCell>
                <TableHeaderCell>Compliant</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {device.browsers.map((browser) => (
                <TableRow key={browser.id}>
                  <TableCell>{browser.name}</TableCell>
                  <TableCell>{browser.version}</TableCell>
                  <TableCell>
                    <Badge appearance="filled" color={browser.isCompliant ? 'success' : 'danger'}>
                      {browser.isCompliant ? 'Yes' : 'No'}
                    </Badge>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Card>
      )}
    </div>
  );
}

function DetailRow({ label, value }: { label: string; value: string }) {
  const styles = makeStyles({
    row: {
      display: 'flex',
      justifyContent: 'space-between',
      padding: `${tokens.spacingVerticalXS} 0`,
    },
    label: {
      color: tokens.colorNeutralForeground3,
    },
  })();

  return (
    <div className={styles.row}>
      <Text className={styles.label}>{label}</Text>
      <Text>{value}</Text>
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

  return <Badge appearance="filled" color={color} size="large">{state}</Badge>;
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
