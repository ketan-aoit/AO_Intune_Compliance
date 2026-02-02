import type { ReactNode } from 'react';
import { useDevAuth } from './DevAuthProvider';

interface DevAuthGuardProps {
  children: ReactNode;
}

/**
 * Development auth guard that always allows access when dev auth is enabled
 */
export function DevAuthGuard({ children }: DevAuthGuardProps) {
  const { isAuthenticated } = useDevAuth();

  if (!isAuthenticated) {
    return (
      <div style={{ padding: '20px', textAlign: 'center' }}>
        <h2>Dev Auth Not Configured</h2>
        <p>Dev auth is not properly configured. Check your environment variables.</p>
      </div>
    );
  }

  return <>{children}</>;
}
