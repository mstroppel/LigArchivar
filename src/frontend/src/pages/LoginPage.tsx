import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { login, UnauthorizedError } from '../api/archiveApi';
import type { LoginRequestDto } from '../types/archive';
import styles from './LoginPage.module.css';

export function LoginPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const queryClient = useQueryClient();

  const loginMutation = useMutation({
    mutationFn: (credentials: LoginRequestDto) => login(credentials),
    onSuccess: () => {
      setError(null);
      // Invalidate the auth status query so App re-checks
      queryClient.invalidateQueries({ queryKey: ['auth-status'] });
    },
    onError: (err) => {
      if (err instanceof UnauthorizedError) {
        setError('Ungültiger Benutzername oder Passwort.');
      } else {
        setError('Verbindungsfehler. Bitte versuchen Sie es erneut.');
      }
    },
  });

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!username || !password) return;
    loginMutation.mutate({ username, password });
  }

  return (
    <div className={styles.container}>
      <div className={styles.card}>
        <h1 className={styles.title}>LigArchivar</h1>
        <form onSubmit={handleSubmit} className={styles.form}>
          <div className={styles.field}>
            <label htmlFor="username" className={styles.label}>
              Benutzername
            </label>
            <input
              id="username"
              type="text"
              className={styles.input}
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              autoComplete="username"
              autoFocus
              disabled={loginMutation.isPending}
            />
          </div>
          <div className={styles.field}>
            <label htmlFor="password" className={styles.label}>
              Passwort
            </label>
            <input
              id="password"
              type="password"
              className={styles.input}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete="current-password"
              disabled={loginMutation.isPending}
            />
          </div>
          {error && <p className={styles.error}>{error}</p>}
          <button
            type="submit"
            className={styles.button}
            disabled={loginMutation.isPending || !username || !password}
          >
            {loginMutation.isPending ? 'Anmelden…' : 'Anmelden'}
          </button>
        </form>
      </div>
    </div>
  );
}
