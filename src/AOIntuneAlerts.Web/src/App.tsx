import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { FluentProvider, webLightTheme } from '@fluentui/react-components';
import { AuthProvider, AuthGuard } from './auth';
import { DevAuthProvider } from './auth/DevAuthProvider';
import { DevAuthGuard } from './auth/DevAuthGuard';
import { isDevAuthEnabled } from './auth/devAuth';
import { AppRoutes } from './app/routes';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5, // 5 minutes
      retry: 1,
    },
  },
});

// Check if dev auth is enabled for Playwright testing
const devAuthEnabled = isDevAuthEnabled();

if (devAuthEnabled) {
  console.log('[App] Dev auth mode enabled - MSAL bypassed');
}

function App() {
  // Use dev auth provider when dev auth is enabled
  if (devAuthEnabled) {
    return (
      <FluentProvider theme={webLightTheme}>
        <QueryClientProvider client={queryClient}>
          <DevAuthProvider>
            <BrowserRouter>
              <DevAuthGuard>
                <AppRoutes />
              </DevAuthGuard>
            </BrowserRouter>
          </DevAuthProvider>
        </QueryClientProvider>
      </FluentProvider>
    );
  }

  // Normal MSAL authentication
  return (
    <FluentProvider theme={webLightTheme}>
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <BrowserRouter>
            <AuthGuard>
              <AppRoutes />
            </AuthGuard>
          </BrowserRouter>
        </AuthProvider>
      </QueryClientProvider>
    </FluentProvider>
  );
}

export default App;
