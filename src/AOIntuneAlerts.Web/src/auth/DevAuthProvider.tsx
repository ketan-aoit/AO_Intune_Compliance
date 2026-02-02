import { createContext, useContext, type ReactNode } from 'react';
import { getDevAuthConfig, getDevUser } from './devAuth';

interface DevAuthContextValue {
  isAuthenticated: boolean;
  user: ReturnType<typeof getDevUser> | null;
  login: () => void;
  logout: () => void;
}

const DevAuthContext = createContext<DevAuthContextValue | null>(null);

interface DevAuthProviderProps {
  children: ReactNode;
}

/**
 * Development authentication provider that bypasses MSAL
 * WARNING: Only use for development/testing!
 */
export function DevAuthProvider({ children }: DevAuthProviderProps) {
  const config = getDevAuthConfig();
  const user = config.enabled ? getDevUser() : null;

  const value: DevAuthContextValue = {
    isAuthenticated: config.enabled,
    user,
    login: () => {
      console.log('[DevAuth] Login bypassed - already authenticated in dev mode');
    },
    logout: () => {
      console.log('[DevAuth] Logout - redirecting to home');
      window.location.href = '/';
    },
  };

  return (
    <DevAuthContext.Provider value={value}>
      {children}
    </DevAuthContext.Provider>
  );
}

export function useDevAuth() {
  const context = useContext(DevAuthContext);
  if (!context) {
    throw new Error('useDevAuth must be used within a DevAuthProvider');
  }
  return context;
}
