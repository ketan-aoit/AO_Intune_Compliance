import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  makeStyles,
  tokens,
  Input,
  Button,
  Dropdown,
  Option,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  Spinner,
  Text,
  Badge,
} from '@fluentui/react-components';
import { Search20Regular, ArrowSync20Regular } from '@fluentui/react-icons';
import { useDevices, useSyncDevices, type GetDevicesParams } from '../api';
import { useAuth } from '../auth';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    gap: tokens.spacingVerticalL,
  },
  toolbar: {
    display: 'flex',
    gap: tokens.spacingHorizontalM,
    flexWrap: 'wrap',
    alignItems: 'center',
  },
  searchInput: {
    minWidth: '300px',
  },
  tableRow: {
    cursor: 'pointer',
    '&:hover': {
      backgroundColor: tokens.colorNeutralBackground1Hover,
    },
  },
  pagination: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginTop: tokens.spacingVerticalM,
  },
});

const complianceStates = [
  { value: '', label: 'All States' },
  { value: 'Compliant', label: 'Compliant' },
  { value: 'NonCompliant', label: 'Non-Compliant' },
  { value: 'ApproachingEndOfSupport', label: 'Approaching EOS' },
  { value: 'InGracePeriod', label: 'In Grace Period' },
  { value: 'ConfigManager', label: 'Config Manager' },
  { value: 'Conflict', label: 'Conflict' },
  { value: 'Error', label: 'Error' },
  { value: 'Unknown', label: 'Unknown' },
];

export function Devices() {
  const styles = useStyles();
  const navigate = useNavigate();
  const { isAdmin } = useAuth();

  const [params, setParams] = useState<GetDevicesParams>({
    pageNumber: 1,
    pageSize: 20,
    searchTerm: '',
    complianceState: '',
  });

  const { data, isLoading, refetch } = useDevices(params);
  const syncMutation = useSyncDevices();

  const handleSearch = (value: string) => {
    setParams((prev) => ({ ...prev, searchTerm: value, pageNumber: 1 }));
  };

  const handleComplianceFilter = (value: string) => {
    setParams((prev) => ({
      ...prev,
      complianceState: value || undefined,
      pageNumber: 1,
    }));
  };

  const handleSync = async () => {
    await syncMutation.mutateAsync();
    refetch();
  };

  const handlePageChange = (page: number) => {
    setParams((prev) => ({ ...prev, pageNumber: page }));
  };

  return (
    <div className={styles.container}>
      <div className={styles.toolbar}>
        <Input
          className={styles.searchInput}
          contentBefore={<Search20Regular />}
          placeholder="Search devices..."
          value={params.searchTerm}
          onChange={(_, data) => handleSearch(data.value)}
        />
        <Dropdown
          placeholder="Filter by state"
          value={params.complianceState || 'All States'}
          onOptionSelect={(_, data) => handleComplianceFilter(data.optionValue as string)}
        >
          {complianceStates.map((state) => (
            <Option key={state.value} value={state.value}>
              {state.label}
            </Option>
          ))}
        </Dropdown>
        {isAdmin() && (
          <Button
            icon={<ArrowSync20Regular />}
            onClick={handleSync}
            disabled={syncMutation.isPending}
          >
            {syncMutation.isPending ? 'Syncing...' : 'Sync from Intune'}
          </Button>
        )}
      </div>

      {isLoading ? (
        <Spinner size="large" label="Loading devices..." />
      ) : !data || data.items.length === 0 ? (
        <Text>No devices found</Text>
      ) : (
        <>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHeaderCell>Device Name</TableHeaderCell>
                <TableHeaderCell>User</TableHeaderCell>
                <TableHeaderCell>OS</TableHeaderCell>
                <TableHeaderCell>Compliance</TableHeaderCell>
                <TableHeaderCell>Issues</TableHeaderCell>
                <TableHeaderCell>Last Sync</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.items.map((device) => (
                <TableRow
                  key={device.id}
                  className={styles.tableRow}
                  onClick={() => navigate(`/devices/${device.id}`)}
                >
                  <TableCell>{device.deviceName}</TableCell>
                  <TableCell>{device.userDisplayName || device.userPrincipalName || '-'}</TableCell>
                  <TableCell>{device.operatingSystem} {device.osVersion}</TableCell>
                  <TableCell>
                    <ComplianceBadge state={device.complianceState} />
                  </TableCell>
                  <TableCell>
                    {device.complianceIssueCount > 0 ? (
                      <Badge appearance="filled" color="danger">{device.complianceIssueCount}</Badge>
                    ) : (
                      '-'
                    )}
                  </TableCell>
                  <TableCell>
                    {device.lastSyncDateTime
                      ? new Date(device.lastSyncDateTime).toLocaleDateString()
                      : '-'}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>

          <div className={styles.pagination}>
            <Text>
              Showing {(data.pageNumber - 1) * 20 + 1} - {Math.min(data.pageNumber * 20, data.totalCount)} of {data.totalCount}
            </Text>
            <div style={{ display: 'flex', gap: tokens.spacingHorizontalS }}>
              <Button
                disabled={!data.hasPreviousPage}
                onClick={() => handlePageChange(data.pageNumber - 1)}
              >
                Previous
              </Button>
              <Button
                disabled={!data.hasNextPage}
                onClick={() => handlePageChange(data.pageNumber + 1)}
              >
                Next
              </Button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

function ComplianceBadge({ state }: { state: string }) {
  const getColorAndLabel = (): { color: 'success' | 'danger' | 'warning' | 'informative' | 'severe' | 'subtle' | 'important' | 'brand', label: string } => {
    switch (state) {
      case 'Compliant':
        return { color: 'success', label: 'Compliant' };
      case 'NonCompliant':
        return { color: 'danger', label: 'Non-Compliant' };
      case 'ApproachingEndOfSupport':
        return { color: 'warning', label: 'Approaching EOS' };
      case 'InGracePeriod':
        return { color: 'warning', label: 'In Grace Period' };
      case 'ConfigManager':
        return { color: 'informative', label: 'Config Manager' };
      case 'Conflict':
        return { color: 'severe', label: 'Conflict' };
      case 'Error':
        return { color: 'danger', label: 'Error' };
      case 'Unknown':
      default:
        return { color: 'subtle', label: state || 'Unknown' };
    }
  };

  const { color, label } = getColorAndLabel();
  return <Badge appearance="filled" color={color}>{label}</Badge>;
}
