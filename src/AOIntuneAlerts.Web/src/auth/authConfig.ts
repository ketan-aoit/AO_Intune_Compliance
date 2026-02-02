import { LogLevel, type Configuration } from '@azure/msal-browser';

export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_AD_CLIENT_ID || '',
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AZURE_AD_TENANT_ID || ''}`,
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: 'localStorage',
  },
  system: {
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) return;
        switch (level) {
          case LogLevel.Error:
            console.error(message);
            return;
          case LogLevel.Warning:
            console.warn(message);
            return;
        }
      },
    },
  },
};

export const loginRequest = {
  scopes: [`api://${import.meta.env.VITE_AZURE_AD_CLIENT_ID}/access_as_user`],
};

export const apiConfig = {
  scopes: [`api://${import.meta.env.VITE_AZURE_AD_CLIENT_ID}/access_as_user`],
  baseUrl: import.meta.env.VITE_API_BASE_URL || 'https://localhost:7001',
};
