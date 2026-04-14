import { useState, useCallback } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { getTree, getEvent, logout, UnauthorizedError } from '../api/archiveApi';
import type { EventDetailDto } from '../types/archive';
import { ArchiveTree } from '../components/ArchiveTree';
import { FileList, type SortMode } from '../components/FileList';
import { RenameControls } from '../components/RenameControls';
import styles from './MainView.module.css';

interface MainViewProps {
  onLoggedOut: () => void;
}

export function MainView({ onLoggedOut }: MainViewProps) {
  const [selectedPath, setSelectedPath] = useState<string | null>(null);
  const [sortMode, setSortMode] = useState<SortMode>('name');
  const [fileOrder, setFileOrder] = useState<string[]>([]);
  const queryClient = useQueryClient();

  // ── Tree query ──────────────────────────────────────────────────────────────
  const treeQuery = useQuery({
    queryKey: ['archive-tree'] as const,
    queryFn: () => getTree(),
    staleTime: 30_000,
    retry: (failureCount, error) => {
      if (error instanceof UnauthorizedError) return false;
      return failureCount < 2;
    },
  });

  // ── Event detail query ──────────────────────────────────────────────────────
  const eventQuery = useQuery({
    queryKey: ['event', selectedPath] as const,
    queryFn: () => getEvent(selectedPath!),
    enabled: selectedPath != null,
    staleTime: 10_000,
    retry: (failureCount, error) => {
      if (error instanceof UnauthorizedError) return false;
      return failureCount < 2;
    },
  });

  function handleSelectEvent(path: string) {
    setSelectedPath(path);
    setEventError(null);
  }

  function handleRenamed(updated: EventDetailDto) {
    // Write the rename result directly into the query cache so the UI
    // updates immediately without a round-trip, then refresh the tree.
    queryClient.setQueryData(['event', selectedPath], updated);
    void treeQuery.refetch();
  }

  function handleReload() {
    void treeQuery.refetch();
    if (selectedPath) void eventQuery.refetch();
  }

  const handleOrderChange = useCallback((order: string[]) => {
    setFileOrder(order);
  }, []);

  async function handleLogout() {
    try {
      await logout();
    } finally {
      onLoggedOut();
    }
  }

  const [eventError, setEventError] = useState<string | null>(null);

  const eventErrorMsg = eventQuery.error instanceof Error
    ? eventQuery.error.message
    : eventQuery.error
    ? 'Fehler beim Laden der Veranstaltung.'
    : eventError;

  const treeErrorMsg = treeQuery.error instanceof Error
    ? treeQuery.error.message
    : treeQuery.error
    ? 'Fehler beim Laden des Archivs.'
    : null;

  const eventDetail: EventDetailDto | undefined = eventQuery.data;

  return (
    <div className={styles.layout}>
      {/* ── Header ─────────────────────────────────────────────────────────── */}
      <header className={styles.header}>
        <span className={styles.brand}>LigArchivar</span>
        <div className={styles.headerActions}>
          <button className={styles.logoutBtn} onClick={handleLogout}>
            Abmelden
          </button>
        </div>
      </header>

      {/* ── Body ───────────────────────────────────────────────────────────── */}
      <div className={styles.body}>
        {/* Sidebar: tree */}
        <aside className={styles.sidebar}>
          {treeQuery.isLoading && (
            <p className={styles.loading}>Archiv wird geladen…</p>
          )}
          {treeErrorMsg && (
            <p className={styles.errorText}>{treeErrorMsg}</p>
          )}
          {treeQuery.data && (
            <ArchiveTree
              nodes={treeQuery.data}
              selectedPath={selectedPath}
              onSelectEvent={handleSelectEvent}
              onReload={handleReload}
              isReloading={treeQuery.isFetching || eventQuery.isFetching}
            />
          )}
        </aside>

        {/* Main panel: event details */}
        <main className={styles.main}>
          {!selectedPath && (
            <p className={styles.hint}>
              Veranstaltung im Baum auswählen.
            </p>
          )}

          {selectedPath && eventQuery.isLoading && !eventDetail && (
            <p className={styles.loading}>Veranstaltung wird geladen…</p>
          )}

          {eventErrorMsg && (
            <p className={styles.errorText}>{eventErrorMsg}</p>
          )}

          {eventDetail && (
            <div className={styles.eventPanel}>
              <div className={styles.renameSection}>
                <RenameControls
                  eventPath={eventDetail.path}
                  fileOrder={fileOrder}
                  onRenamed={handleRenamed}
                />
              </div>
              <div className={styles.fileSection}>
                <FileList
                  event={eventDetail}
                  sortMode={sortMode}
                  onSortChange={setSortMode}
                  onOrderChange={handleOrderChange}
                />
              </div>
            </div>
          )}
        </main>
      </div>
    </div>
  );
}
