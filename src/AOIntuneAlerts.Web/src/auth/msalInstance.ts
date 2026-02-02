import { PublicClientApplication } from '@azure/msal-browser';
import { msalConfig } from './authConfig';

// Single shared MSAL instance for the entire application
export const msalInstance = new PublicClientApplication(msalConfig);

let initialized = false;

export async function ensureMsalInitialized(): Promise<void> {
  if (!initialized) {
    await msalInstance.initialize();
    initialized = true;
  }
}
