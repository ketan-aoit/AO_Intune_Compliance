import {
  makeStyles,
  tokens,
  Card,
  CardHeader,
  Title3,
  Text,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  Badge,
  Spinner,
} from '@fluentui/react-components';
import { useComplianceRules } from '../api';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    gap: tokens.spacingVerticalL,
  },
  section: {
    marginBottom: tokens.spacingVerticalL,
  },
});

export function ComplianceRules() {
  const styles = useStyles();
  const { data: rules, isLoading } = useComplianceRules();

  if (isLoading) {
    return <Spinner size="large" label="Loading compliance rules..." />;
  }

  if (!rules || rules.length === 0) {
    return (
      <div className={styles.container}>
        <Text>No compliance rules configured</Text>
      </div>
    );
  }

  // Group rules by rule type
  const rulesByCategory = rules.reduce((acc, rule) => {
    const category = rule.ruleType || 'General';
    if (!acc[category]) {
      acc[category] = [];
    }
    acc[category].push(rule);
    return acc;
  }, {} as Record<string, typeof rules>);

  return (
    <div className={styles.container}>
      {Object.entries(rulesByCategory).map(([category, categoryRules]) => (
        <Card key={category} className={styles.section}>
          <CardHeader header={<Title3>{category}</Title3>} />
          <Table>
            <TableHeader>
              <TableRow>
                <TableHeaderCell>Rule Name</TableHeaderCell>
                <TableHeaderCell>Description</TableHeaderCell>
                <TableHeaderCell>Severity</TableHeaderCell>
                <TableHeaderCell>Status</TableHeaderCell>
              </TableRow>
            </TableHeader>
            <TableBody>
              {categoryRules.map((rule) => (
                <TableRow key={rule.id}>
                  <TableCell>{rule.name}</TableCell>
                  <TableCell>{rule.description}</TableCell>
                  <TableCell>
                    <SeverityBadge severity={rule.severity} />
                  </TableCell>
                  <TableCell>
                    <Badge
                      appearance="filled"
                      color={rule.isEnabled ? 'success' : 'severe'}
                    >
                      {rule.isEnabled ? 'Enabled' : 'Disabled'}
                    </Badge>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Card>
      ))}
    </div>
  );
}

function SeverityBadge({ severity }: { severity: string }) {
  const color =
    severity === 'Critical'
      ? 'danger'
      : severity === 'High'
      ? 'severe'
      : severity === 'Medium'
      ? 'warning'
      : 'informative';

  return (
    <Badge appearance="filled" color={color}>
      {severity}
    </Badge>
  );
}
