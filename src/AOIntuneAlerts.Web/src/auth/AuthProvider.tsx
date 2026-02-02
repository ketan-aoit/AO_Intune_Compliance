import { useState, useEffect, type ReactNode } from 'react';
import { MsalProvider } from '@azure/msal-react';
import { EventType, type EventMessage, type AuthenticationResult } from '@azure/msal-browser';
import { Spinner, makeStyles } from '@fluentui/react-components';
import { msalInstance, ensureMsalInitialized } from './msalInstance';

const useStyles = makeStyles({
  loadingContainer: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    height: '100vh',
  },
});

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const styles = useStyles();
  const [isInitialized, setIsInitialized] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const initializeMsal = async () => {
      console.log('[Auth] Starting MSAL initialization...');

      try {
        // Initialize the shared MSAL instance
        await ensureMsalInitialized();
        console.log('[Auth] MSAL initialized');

        // Handle redirect promise
        console.log('[Auth] Handling redirect promise...');
        try {
          const response = await msalInstance.handleRedirectPromise();
          console.log('[Auth] Redirect handled:', response ? 'Got response' : 'No redirect response');

          if (response?.account) {
            console.log('[Auth] Setting active account:', response.account.username);
            msalInstance.setActiveAccount(response.account);
          }
        } catch (redirectError) {
          console.error('[Auth] Redirect error:', redirectError);
          if (redirectError instanceof Error) {
            setError(redirectError.message);
          }
        }

        // Check for existing accounts
        const accounts = msalInstance.getAllAccounts();
        console.log('[Auth] Accounts in cache:', accounts.length);

        if (accounts.length > 0 && !msalInstance.getActiveAccount()) {
          console.log('[Auth] Setting active account from cache:', accounts[0].username);
          msalInstance.setActiveAccount(accounts[0]);
        }

        // Listen for auth events
        msalInstance.addEventCallback((event: EventMessage) => {
          if (event.eventType === EventType.LOGIN_SUCCESS && event.payload) {
            const payload = event.payload as AuthenticationResult;
            console.log('[Auth] Login success:', payload.account?.username);
            msalInstance.setActiveAccount(payload.account);
          }
        });

        console.log('[Auth] Initialization complete');
        setIsInitialized(true);
      } catch (err) {
        console.error('[Auth] Initialization error:', err);
        if (err instanceof Error) {
          setError(err.message);
        }
        setIsInitialized(true);
      }
    };

    initializeMsal();
  }, []);

  if (!isInitialized) {
    return (
      <div className={styles.loadingContainer}>
        <Spinner size="large" label="Initializing..." />
      </div>
    );
  }

  if (error) {
    return (
      <div className={styles.loadingContainer}>
        <div style={{ textAlign: 'center' }}>
          <h2>Authentication Error</h2>
          <p>{error}</p>
          <button onClick={() => { window.location.href = window.location.origin; }}>
            Try Again
          </button>
        </div>
      </div>
    );
  }

  return <MsalProvider instance={msalInstance}>{children}</MsalProvider>;
}
