import type { ReactNode } from 'react';
import { useMsal, useIsAuthenticated } from '@azure/msal-react';
import { InteractionStatus } from '@azure/msal-browser';
import { Button, Spinner, Title1, makeStyles, tokens } from '@fluentui/react-components';
import { loginRequest } from './authConfig';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center',
    height: '100vh',
    gap: tokens.spacingVerticalL,
  },
  title: {
    marginBottom: tokens.spacingVerticalL,
  },
});

interface AuthGuardProps {
  children: ReactNode;
}

export function AuthGuard({ children }: AuthGuardProps) {
  const styles = useStyles();
  const { instance, inProgress } = useMsal();
  const isAuthenticated = useIsAuthenticated();

  const handleLogin = () => {
    instance.loginRedirect(loginRequest);
  };

  // Show loading during any MSAL interaction
  if (inProgress !== InteractionStatus.None) {
    return (
      <div className={styles.container}>
        <Spinner size="large" label="Loading..." />
      </div>
    );
  }

  // If authenticated, show the app
  if (isAuthenticated) {
    return <>{children}</>;
  }

  // Not authenticated and no interaction in progress - show login
  return (
    <div className={styles.container}>
      <Title1 className={styles.title}>Intune Compliance Portal</Title1>
      <Button appearance="primary" size="large" onClick={handleLogin}>
        Sign in with Microsoft
      </Button>
    </div>
  );
}
