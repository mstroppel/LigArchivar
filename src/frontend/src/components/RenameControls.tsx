import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { renameEvent, renameEventByDateTime, ConflictError, ApiError } from '../api/archiveApi';
import type { EventDetailDto, RenameRequestDto } from '../types/archive';
import styles from './RenameControls.module.css';

interface RenameControlsProps {
  eventPath: string;
  /** Current display order of file names (from FileList sort state) */
  fileOrder: string[];
  onRenamed: (updated: EventDetailDto) => void;
}

export function RenameControls({ eventPath, fileOrder, onRenamed }: RenameControlsProps) {
  const [startNumber, setStartNumber] = useState(1);
  const [error, setError] = useState<string | null>(null);

  const renameMutation = useMutation({
    mutationFn: (req: RenameRequestDto) => renameEvent(eventPath, req),
    onSuccess: (updated) => {
      setError(null);
      onRenamed(updated);
    },
    onError: (err) => {
      if (err instanceof ConflictError) {
        setError('Umbenennung bereits in Bearbeitung. Bitte warten.');
      } else if (err instanceof ApiError) {
        setError(err.message || `Fehler ${err.status}`);
      } else if (err instanceof Error) {
        setError('Verbindungsfehler: ' + err.message);
      } else {
        setError('Unbekannter Fehler.');
      }
    },
  });

  const renameDateMutation = useMutation({
    mutationFn: () => renameEventByDateTime(eventPath),
    onSuccess: (updated) => {
      setError(null);
      onRenamed(updated);
    },
    onError: (err) => {
      if (err instanceof ConflictError) {
        setError('Umbenennung bereits in Bearbeitung. Bitte warten.');
      } else if (err instanceof ApiError) {
        setError(err.message || `Fehler ${err.status}`);
      } else if (err instanceof Error) {
        setError('Verbindungsfehler: ' + err.message);
      } else {
        setError('Unbekannter Fehler.');
      }
    },
  });

  const isBusy = renameMutation.isPending || renameDateMutation.isPending;

  function handleRename() {
    setError(null);
    renameMutation.mutate({
      startNumber,
      fileOrder: fileOrder.length > 0 ? fileOrder : undefined,
    });
  }

  function handleRenameByDate() {
    setError(null);
    renameDateMutation.mutate();
  }

  return (
    <div className={styles.container}>
      <div className={styles.row}>
        <div className={styles.startNumberGroup}>
          <label htmlFor="startNumber" className={styles.label}>
            Startnummer
          </label>
          <input
            id="startNumber"
            type="number"
            min={1}
            max={9999}
            className={styles.numberInput}
            value={startNumber}
            onChange={(e) => setStartNumber(Math.max(1, parseInt(e.target.value, 10) || 1))}
            disabled={isBusy}
          />
        </div>

        <button
          className={[styles.btn, styles.btnPrimary].join(' ')}
          onClick={handleRename}
          disabled={isBusy}
          title="Dateien fortlaufend umnummerieren (in aktueller Sortierreihenfolge)"
        >
          {renameMutation.isPending ? 'Umbenennen…' : 'Umbenennen'}
        </button>

        <button
          className={[styles.btn, styles.btnSecondary].join(' ')}
          onClick={handleRenameByDate}
          disabled={isBusy}
          title="Dateien nach Dateidatum umbenennen"
        >
          {renameDateMutation.isPending ? 'Nach Datum…' : 'Nach Datum umbenennen'}
        </button>
      </div>

      {error && (
        <div className={styles.errorBanner} role="alert">
          <strong>Fehler:</strong> {error}
          <button
            className={styles.dismissBtn}
            onClick={() => setError(null)}
            aria-label="Fehlermeldung schließen"
          >
            ✕
          </button>
        </div>
      )}
    </div>
  );
}
