import {
  makeStyles,
  tokens,
  Card,
  CardHeader,
  Title3,
  Text,
  Switch,
  Input,
  Button,
  Divider,
} from '@fluentui/react-components';
import { useAuth } from '../auth';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    gap: tokens.spacingVerticalL,
    maxWidth: '800px',
  },
  section: {
    display: 'flex',
    flexDirection: 'column',
    gap: tokens.spacingVerticalM,
    padding: tokens.spacingVerticalM,
  },
  settingRow: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: `${tokens.spacingVerticalS} 0`,
  },
  settingLabel: {
    display: 'flex',
    flexDirection: 'column',
    gap: tokens.spacingVerticalXS,
  },
  inputGroup: {
    display: 'flex',
    gap: tokens.spacingHorizontalM,
    alignItems: 'center',
  },
});

export function Settings() {
  const styles = useStyles();
  const { user } = useAuth();

  return (
    <div className={styles.container}>
      <Card>
        <CardHeader header={<Title3>User Information</Title3>} />
        <div className={styles.section}>
          <div className={styles.settingRow}>
            <div className={styles.settingLabel}>
              <Text weight="semibold">Name</Text>
              <Text size={200}>{user?.name || 'Not available'}</Text>
            </div>
          </div>
          <Divider />
          <div className={styles.settingRow}>
            <div className={styles.settingLabel}>
              <Text weight="semibold">Email</Text>
              <Text size={200}>{user?.username || 'Not available'}</Text>
            </div>
          </div>
        </div>
      </Card>

      <Card>
        <CardHeader header={<Title3>Notification Settings</Title3>} />
        <div className={styles.section}>
          <div className={styles.settingRow}>
            <div className={styles.settingLabel}>
              <Text weight="semibold">Email Notifications</Text>
              <Text size={200}>Receive email alerts for compliance issues</Text>
            </div>
            <Switch defaultChecked />
          </div>
          <Divider />
          <div className={styles.settingRow}>
            <div className={styles.settingLabel}>
              <Text weight="semibold">Daily Summary</Text>
              <Text size={200}>Receive a daily compliance summary report</Text>
            </div>
            <Switch />
          </div>
        </div>
      </Card>

      <Card>
        <CardHeader header={<Title3>System Settings</Title3>} />
        <div className={styles.section}>
          <div className={styles.settingRow}>
            <div className={styles.settingLabel}>
              <Text weight="semibold">Sync Interval (hours)</Text>
              <Text size={200}>How often to sync devices from Intune</Text>
            </div>
            <div className={styles.inputGroup}>
              <Input type="number" defaultValue="4" style={{ width: '80px' }} />
              <Button appearance="primary">Save</Button>
            </div>
          </div>
          <Divider />
          <div className={styles.settingRow}>
            <div className={styles.settingLabel}>
              <Text weight="semibold">Alert Threshold (days)</Text>
              <Text size={200}>Days before end-of-support to trigger alerts</Text>
            </div>
            <div className={styles.inputGroup}>
              <Input type="number" defaultValue="90" style={{ width: '80px' }} />
              <Button appearance="primary">Save</Button>
            </div>
          </div>
        </div>
      </Card>
    </div>
  );
}
