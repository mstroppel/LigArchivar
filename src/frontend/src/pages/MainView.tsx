import { useState, useCallback, useRef } from 'react';
import { useQuery } from '@tanstack/react-query';
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
  const [eventDetail, setEventDetail] = useState<EventDetailDto | null>(null);
  const [sortMode, setSortMode] = useState<SortMode>('name');
  const [eventError, setEventError] = useState<string | null>(null);
  const fileOrderRef = useRef<string[]>([]);

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

  // Sync event detail to local state (so rename can update it without refetch)
  if (eventQuery.data && eventQuery.data !== eventDetail) {
    setEventDetail(eventQuery.data);
  }
  if (eventQuery.error) {
    const msg =
      eventQuery.error instanceof Error
        ? eventQuery.error.message
        : 'Fehler beim Laden der Veranstaltung.';
    if (msg !== eventError) setEventError(msg);
  } else if (eventError && !eventQuery.error) {
    setEventError(null);
  }

  function handleSelectEvent(path: string) {
    setSelectedPath(path);
    setEventDetail(null);
    setEventError(null);
  }

  function handleRenamed(updated: EventDetailDto) {
    setEventDetail(updated);
  }

  const handleOrderChange = useCallback((order: string[]) => {
    fileOrderRef.current = order;
  }, []);

  async function handleLogout() {
    try {
      await logout();
    } finally {
      onLoggedOut();
    }
  }

  const treeErrorMsg = treeQuery.error instanceof Error
    ? treeQuery.error.message
    : treeQuery.error
    ? 'Fehler beim Laden des Archivs.'
    : null;

  return (
    <div className={styles.layout}>
      {/* ── Header ─────────────────────────────────────────────────────────── */}
      <header className={styles.header}>
        <span className={styles.brand}>LigArchivar</span>
        <button className={styles.logoutBtn} onClick={handleLogout}>
          Abmelden
        </button>
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

          {eventError && (
            <p className={styles.errorText}>{eventError}</p>
          )}

          {eventDetail && (
            <div className={styles.eventPanel}>
              <div className={styles.renameSection}>
                <RenameControls
                  eventPath={eventDetail.path}
                  fileOrder={fileOrderRef.current}
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
