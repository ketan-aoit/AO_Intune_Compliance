import { useState } from 'react';
import {
  makeStyles,
  tokens,
  Card,
  CardHeader,
  Title3,
  Text,
  Button,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  Badge,
  Spinner,
  Tab,
  TabList,
} from '@fluentui/react-components';
import { useAlertRecipients, useAlertHistory, type GetAlertHistoryParams } from '../api';
import { useAuth } from '../auth';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    gap: tokens.spacingVerticalL,
  },
  pagination: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginTop: tokens.spacingVerticalM,
  },
});

export function Alerts() {
  const styles = useStyles();
  const [selectedTab, setSelectedTab] = useState('history');
  const { isManager } = useAuth();

  return (
    <div className={styles.container}>
      <TabList
        selectedValue={selectedTab}
        onTabSelect={(_, data) => setSelectedTab(data.value as string)}
      >
        <Tab value="history">Alert History</Tab>
        {isManager() && <Tab value="recipients">Recipients</Tab>}
      </TabList>

      {selectedTab === 'history' && <AlertHistory />}
      {selectedTab === 'recipients' && isManager() && <AlertRecipients />}
    </div>
  );
}

function AlertHistory() {
  const styles = makeStyles({
    pagination: {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      marginTop: tokens.spacingVerticalM,
    },
  })();

  const [params, setParams] = useState<GetAlertHistoryParams>({
    pageNumber: 1,
    pageSize: 20,
  });

  const { data, isLoading } = useAlertHistory(params);

  const handlePageChange = (page: number) => {
    setParams((prev) => ({ ...prev, pageNumber: page }));
  };

  if (isLoading) {
    return <Spinner size="large" label="Loading alerts..." />;
  }

  if (!data || data.items.length === 0) {
    return <Text>No alerts found</Text>;
  }

  return (
    <>
      <Table>
        <TableHeader>
          <TableRow>
            <TableHeaderCell>Subject</TableHeaderCell>
            <TableHeaderCell>Severity</TableHeaderCell>
            <TableHeaderCell>Status</TableHeaderCell>
            <TableHeaderCell>Recipients</TableHeaderCell>
            <TableHeaderCell>Sent At</TableHeaderCell>
          </TableRow>
        </TableHeader>
        <TableBody>
          {data.items.map((alert) => (
            <TableRow key={alert.id}>
              <TableCell>{alert.subject}</TableCell>
              <TableCell>
                <SeverityBadge severity={alert.severity} />
              </TableCell>
              <TableCell>
                <Badge appearance="filled" color={alert.wasSent ? 'success' : 'danger'}>
                  {alert.wasSent ? 'Sent' : 'Failed'}
                </Badge>
              </TableCell>
              <TableCell>{alert.recipientCount}</TableCell>
              <TableCell>
                {alert.sentAt ? new Date(alert.sentAt).toLocaleString() : '-'}
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
  );
}

function AlertRecipients() {
  const { data: recipients, isLoading } = useAlertRecipients();

  if (isLoading) {
    return <Spinner size="large" label="Loading recipients..." />;
  }

  if (!recipients || recipients.length === 0) {
    return <Text>No recipients configured</Text>;
  }

  return (
    <Card>
      <CardHeader header={<Title3>Alert Recipients</Title3>} />
      <Table>
        <TableHeader>
          <TableRow>
            <TableHeaderCell>Name</TableHeaderCell>
            <TableHeaderCell>Email</TableHeaderCell>
            <TableHeaderCell>Min. Severity</TableHeaderCell>
            <TableHeaderCell>Status</TableHeaderCell>
          </TableRow>
        </TableHeader>
        <TableBody>
          {recipients.map((recipient) => (
            <TableRow key={recipient.id}>
              <TableCell>{recipient.displayName}</TableCell>
              <TableCell>{recipient.email}</TableCell>
              <TableCell>
                <SeverityBadge severity={recipient.minimumSeverity} />
              </TableCell>
              <TableCell>
                <Badge appearance="filled" color={recipient.isEnabled ? 'success' : 'severe'}>
                  {recipient.isEnabled ? 'Active' : 'Disabled'}
                </Badge>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Card>
  );
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
