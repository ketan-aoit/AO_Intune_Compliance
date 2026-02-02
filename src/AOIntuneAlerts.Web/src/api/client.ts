import axios, { type AxiosInstance, type InternalAxiosRequestConfig } from 'axios';
import { InteractionRequiredAuthError } from '@azure/msal-browser';
import { msalInstance, ensureMsalInitialized } from '../auth/msalInstance';
import { loginRequest, apiConfig } from '../auth/authConfig';
import { isDevAuthEnabled, getDevAuthHeaders } from '../auth/devAuth';

const apiClient: AxiosInstance = axios.create({
  baseURL: `${apiConfig.baseUrl}/api/v1`,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use(
  async (config: InternalAxiosRequestConfig) => {
    // Check if dev auth is enabled (for Playwright testing)
    if (isDevAuthEnabled()) {
      console.log('[API] Using dev auth bypass');
      const devHeaders = getDevAuthHeaders();
      Object.entries(devHeaders).forEach(([key, value]) => {
        config.headers.set(key, value);
      });
      return config;
    }

    try {
      // Ensure MSAL is initialized (uses shared instance)
      await ensureMsalInitialized();

      const accounts = msalInstance.getAllAccounts();
      console.log('[API] Accounts available:', accounts.length);

      if (accounts.length > 0) {
        const account = msalInstance.getActiveAccount() || accounts[0];
        console.log('[API] Using account:', account.username);

        try {
          const response = await msalInstance.acquireTokenSilent({
            ...loginRequest,
            account: account,
          });
          console.log('[API] Token acquired silently');
          config.headers.Authorization = `Bearer ${response.accessToken}`;
        } catch (error) {
          if (error instanceof InteractionRequiredAuthError) {
            console.log('[API] Interactive auth required, redirecting...');
            await msalInstance.acquireTokenRedirect(loginRequest);
          } else {
            console.error('[API] Token acquisition error:', error);
          }
        }
      } else {
        console.warn('[API] No accounts found - user needs to sign in');
      }
    } catch (error) {
      console.error('[API] MSAL error:', error);
    }
    return config;
  },
  (error) => Promise.reject(error)
);

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    console.log('[API] Response error:', error.response?.status);
    if (error.response?.status === 401) {
      console.log('[API] 401 received, checking if re-auth needed...');
      const accounts = msalInstance.getAllAccounts();
      if (accounts.length === 0) {
        console.log('[API] No accounts, redirecting to login...');
        await ensureMsalInitialized();
        msalInstance.loginRedirect(loginRequest);
      }
    }
    return Promise.reject(error);
  }
);

export { apiClient };

// Debug function to test auth
export async function testAuth() {
  try {
    await ensureMsalInitialized();
    const accounts = msalInstance.getAllAccounts();
    console.log('[Debug] Accounts:', accounts.length);

    if (accounts.length > 0) {
      const account = msalInstance.getActiveAccount() || accounts[0];
      const tokenResponse = await msalInstance.acquireTokenSilent({
        ...loginRequest,
        account: account,
      });
      console.log('[Debug] Token acquired, calling debug endpoint...');
      console.log('[Debug] Token audience:', (tokenResponse.idTokenClaims as Record<string, unknown>)?.aud);
      console.log('[Debug] Token scopes:', tokenResponse.scopes);

      const response = await fetch(`${apiConfig.baseUrl}/debug/claims`, {
        headers: {
          'Authorization': `Bearer ${tokenResponse.accessToken}`,
        },
      });

      const data = await response.json();
      console.log('[Debug] Response:', data);
      return data;
    }
  } catch (error) {
    console.error('[Debug] Error:', error);
  }
}

// Make it available globally for console testing
if (typeof window !== 'undefined') {
  (window as any).testAuth = testAuth;
}
