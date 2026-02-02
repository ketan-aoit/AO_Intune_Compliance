import { useMsal, useAccount } from '@azure/msal-react';
import { useMemo } from 'react';
import { loginRequest } from './authConfig';
import { isDevAuthEnabled, getDevAuthConfig, getDevUser } from './devAuth';

// Check once at module load time (before any hooks)
const devAuthEnabled = isDevAuthEnabled();

export function useAuth() {
  // For dev auth, we don't need MSAL hooks at all
  if (devAuthEnabled) {
    return useDevAuthOnly();
  }

  // For MSAL auth, use the hooks
  return useMsalAuthOnly();
}

// Dev auth implementation (no hooks needed)
function useDevAuthOnly() {
  const config = getDevAuthConfig();
  const devUser = getDevUser();

  // Use useMemo to maintain stable references
  return useMemo(() => {
    const getRoles = (): string[] => {
      return [config.role];
    };

    const hasRole = (role: string): boolean => {
      const roles = getRoles();
      return roles.includes(role);
    };

    const isAdmin = () => hasRole('Admin');
    const isManager = () => hasRole('Manager') || isAdmin();
    const isViewer = () => hasRole('Viewer') || isManager();

    const logout = () => {
      console.log('[DevAuth] Logout - redirecting to home');
      window.location.href = '/';
    };

    const getAccessToken = async (): Promise<string> => {
      return 'dev-token';
    };

    return {
      user: {
        name: devUser.name,
        username: devUser.username,
        localAccountId: devUser.localAccountId,
        homeAccountId: devUser.homeAccountId,
        environment: devUser.environment,
        tenantId: devUser.tenantId,
        idTokenClaims: devUser.idTokenClaims,
      },
      isAuthenticated: true,
      roles: getRoles(),
      isAdmin,
      isManager,
      isViewer,
      logout,
      getAccessToken,
    };
  }, [config.role, devUser]);
}

// MSAL auth implementation
function useMsalAuthOnly() {
  const { instance, accounts } = useMsal();
  const account = useAccount(accounts[0] || {});

  const getRoles = (): string[] => {
    if (!account?.idTokenClaims) return [];
    const roles = account.idTokenClaims['roles'] as string[] | undefined;
    return roles || [];
  };

  const hasRole = (role: string): boolean => {
    const roles = getRoles();
    return roles.includes(role);
  };

  const isAdmin = () => hasRole('Admin');
  const isManager = () => hasRole('Manager') || isAdmin();
  const isViewer = () => hasRole('Viewer') || isManager();

  const logout = () => {
    instance.logoutRedirect({
      postLogoutRedirectUri: window.location.origin,
    });
  };

  const getAccessToken = async (): Promise<string> => {
    const response = await instance.acquireTokenSilent({
      ...loginRequest,
      account: accounts[0],
    });
    return response.accessToken;
  };

  return {
    user: account,
    isAuthenticated: !!account,
    roles: getRoles(),
    isAdmin,
    isManager,
    isViewer,
    logout,
    getAccessToken,
  };
}
