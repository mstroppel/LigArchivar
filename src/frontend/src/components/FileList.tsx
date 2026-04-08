import { useMemo } from 'react';
import type { EventDetailDto, FileGroupDto } from '../types/archive';
import styles from './FileList.module.css';

export type SortMode = 'name' | 'date';

interface FileListProps {
  event: EventDetailDto;
  sortMode: SortMode;
  onSortChange: (mode: SortMode) => void;
  /** Ordered list of file names after sorting — communicated back to parent for rename */
  onOrderChange: (order: string[]) => void;
}

export function FileList({ event, sortMode, onSortChange, onOrderChange }: FileListProps) {
  const sortedFiles = useMemo(() => {
    const sorted = [...event.files].sort((a, b) => {
      if (sortMode === 'date') {
        return (
          new Date(a.lastWriteTimeUtc).getTime() - new Date(b.lastWriteTimeUtc).getTime()
        );
      }
      return a.name.localeCompare(b.name);
    });
    // Notify parent of current order
    onOrderChange(sorted.map((f) => f.name));
    return sorted;
  // onOrderChange is stable (passed from parent) — only re-run when files or sortMode changes
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [event.files, sortMode]);

  function formatDate(isoString: string) {
    const d = new Date(isoString);
    return d.toLocaleString('de-DE', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    });
  }

  return (
    <div className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.eventName}>{event.name}</h2>
        <div className={styles.sortControls}>
          <span className={styles.sortLabel}>Sortierung:</span>
          <button
            className={[styles.sortBtn, sortMode === 'name' ? styles.sortActive : ''].join(' ')}
            onClick={() => onSortChange('name')}
            title="Nach Name sortieren"
          >
            Name
          </button>
          <button
            className={[styles.sortBtn, sortMode === 'date' ? styles.sortActive : ''].join(' ')}
            onClick={() => onSortChange('date')}
            title="Nach Datum sortieren"
          >
            Datum
          </button>
        </div>
      </div>

      {event.files.length === 0 ? (
        <p className={styles.empty}>Keine Dateien vorhanden.</p>
      ) : (
        <table className={styles.table}>
          <thead>
            <tr>
              <th className={styles.th}>Name</th>
              <th className={styles.th}>Erweiterungen</th>
              <th className={styles.th}>Eigenschaften</th>
              <th className={styles.th}>Datum (UTC)</th>
              <th className={styles.th}>Status</th>
            </tr>
          </thead>
          <tbody>
            {sortedFiles.map((file) => (
              <FileRow key={file.name} file={file} formatDate={formatDate} />
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

interface FileRowProps {
  file: FileGroupDto;
  formatDate: (s: string) => string;
}

function FileRow({ file, formatDate }: FileRowProps) {
  const rowClass = [
    styles.tr,
    !file.isValid ? styles.invalidRow : '',
    file.isOrphaned ? styles.orphanedRow : '',
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <tr className={rowClass}>
      <td className={styles.td}>{file.name}</td>
      <td className={styles.td}>
        {file.extensions.map((ext) => (
          <span key={ext} className={styles.tag}>
            {ext}
          </span>
        ))}
      </td>
      <td className={styles.td}>
        {file.properties.map((prop) => (
          <span key={prop} className={styles.tag}>
            {prop}
          </span>
        ))}
      </td>
      <td className={styles.td}>{formatDate(file.lastWriteTimeUtc)}</td>
      <td className={styles.td}>
        {file.isOrphaned && (
          <span className={styles.statusOrphaned} title="Verwaiste DNG-Datei (keine JPG)">
            verwaist
          </span>
        )}
        {!file.isValid && !file.isOrphaned && (
          <span className={styles.statusInvalid} title="Ungültiger Dateiname">
            ungültig
          </span>
        )}
        {file.isValid && !file.isOrphaned && (
          <span className={styles.statusOk}>ok</span>
        )}
      </td>
    </tr>
  );
}
