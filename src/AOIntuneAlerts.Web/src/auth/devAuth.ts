/**
 * Development authentication bypass
 * WARNING: This should NEVER be used in production!
 *
 * When DEV_AUTH is enabled:
 * 1. MSAL authentication is bypassed
 * 2. API requests use X-Dev-Auth header instead of Bearer token
 * 3. User info comes from environment variables
 */

export interface DevAuthConfig {
  enabled: boolean;
  secret: string;
  user: string;
  role: string;
}

// Check if dev auth is enabled via environment variables
export function getDevAuthConfig(): DevAuthConfig {
  const enabled = import.meta.env.VITE_DEV_AUTH_ENABLED === 'true';
  const secret = import.meta.env.VITE_DEV_AUTH_SECRET || '';
  const user = import.meta.env.VITE_DEV_AUTH_USER || 'test@example.com';
  const role = import.meta.env.VITE_DEV_AUTH_ROLE || 'Admin';

  return { enabled, secret, user, role };
}

// Check if running in dev auth mode
export function isDevAuthEnabled(): boolean {
  return getDevAuthConfig().enabled;
}

// Get dev auth headers for API requests
export function getDevAuthHeaders(): Record<string, string> {
  const config = getDevAuthConfig();
  if (!config.enabled) {
    return {};
  }

  return {
    'X-Dev-Auth': config.secret,
    'X-Dev-User': config.user,
    'X-Dev-Role': config.role,
  };
}

// Mock user for dev auth mode
export function getDevUser() {
  const config = getDevAuthConfig();
  return {
    username: config.user,
    name: config.user.split('@')[0],
    localAccountId: 'dev-user-001',
    homeAccountId: 'dev-user-001',
    environment: 'dev',
    tenantId: '00000000-0000-0000-0000-000000000000',
    idTokenClaims: {
      name: config.user.split('@')[0],
      preferred_username: config.user,
      roles: [config.role],
    },
  };
}
