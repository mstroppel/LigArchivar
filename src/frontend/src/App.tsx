import { useQuery, useQueryClient } from '@tanstack/react-query';
import { getAuthStatus } from './api/archiveApi';
import { LoginPage } from './pages/LoginPage';
import { MainView } from './pages/MainView';

export function App() {
  const queryClient = useQueryClient();

  const authQuery = useQuery({
    queryKey: ['auth-status'],
    queryFn: getAuthStatus,
    staleTime: 60_000,
    retry: false,
  });

  function handleLoggedOut() {
    // Set auth status to unauthenticated immediately so the login page
    // renders synchronously, then clear the rest of the cache.
    queryClient.setQueryData(['auth-status'], { authenticated: false });
    queryClient.removeQueries({ queryKey: ['archive-tree'] });
    queryClient.removeQueries({ queryKey: ['event'] });
  }

  if (authQuery.isLoading) {
    return (
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100vh' }}>
        <span style={{ color: 'var(--color-text-muted)', fontSize: '0.9rem' }}>Laden…</span>
      </div>
    );
  }

  const isAuthenticated = authQuery.data?.authenticated === true;

  if (!isAuthenticated) {
    return <LoginPage />;
  }

  return <MainView onLoggedOut={handleLoggedOut} />;
}
