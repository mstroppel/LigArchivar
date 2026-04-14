import { useState } from 'react';
import type { TreeNodeDto } from '../types/archive';
import styles from './ArchiveTree.module.css';

interface ArchiveTreeProps {
  nodes: TreeNodeDto[];
  selectedPath: string | null;
  onSelectEvent: (path: string) => void;
  onReload?: () => void;
  isReloading?: boolean;
}

function collectBranchPaths(nodes: TreeNodeDto[]): string[] {
  const paths: string[] = [];
  for (const node of nodes) {
    if (node.children && node.children.length > 0) {
      paths.push(node.path);
      paths.push(...collectBranchPaths(node.children));
    }
  }
  return paths;
}

export function ArchiveTree({ nodes, selectedPath, onSelectEvent, onReload, isReloading }: ArchiveTreeProps) {
  const [expandedPaths, setExpandedPaths] = useState<ReadonlySet<string>>(new Set());

  function handleToggle(path: string) {
    setExpandedPaths((prev) => {
      const next = new Set(prev);
      if (next.has(path)) next.delete(path);
      else next.add(path);
      return next;
    });
  }

  function handleExpandAll() {
    setExpandedPaths(new Set(collectBranchPaths(nodes)));
  }

  function handleCollapseAll() {
    setExpandedPaths(new Set());
  }

  return (
    <div>
      <div className={styles.treeControls}>
        <button
          className={styles.treeBtn}
          onClick={handleExpandAll}
          title="Alle aufklappen"
        >
          ▾ Alle
        </button>
        <button
          className={styles.treeBtn}
          onClick={handleCollapseAll}
          title="Alle zuklappen"
        >
          ▸ Keine
        </button>
        {onReload && (
          <button
            className={styles.treeBtn}
            onClick={onReload}
            disabled={isReloading}
            title="Archiv neu laden"
          >
            {isReloading ? 'Laden…' : 'Neu laden'}
          </button>
        )}
      </div>
      <ul className={styles.tree} role="tree">
        {nodes.map((node) => (
          <TreeNode
            key={node.path}
            node={node}
            selectedPath={selectedPath}
            onSelectEvent={onSelectEvent}
            expandedPaths={expandedPaths}
            onToggle={handleToggle}
            depth={0}
          />
        ))}
      </ul>
    </div>
  );
}

interface TreeNodeProps {
  node: TreeNodeDto;
  selectedPath: string | null;
  onSelectEvent: (path: string) => void;
  expandedPaths: ReadonlySet<string>;
  onToggle: (path: string) => void;
  depth: number;
}

function TreeNode({ node, selectedPath, onSelectEvent, expandedPaths, onToggle, depth }: TreeNodeProps) {
  const expanded = expandedPaths.has(node.path);
  const hasChildren = node.children && node.children.length > 0;
  const isSelected = node.path === selectedPath;
  const isEvent = node.nodeType === 'event';

  function handleClick() {
    if (isEvent) {
      onSelectEvent(node.path);
    } else if (hasChildren) {
      onToggle(node.path);
    }
  }

  function handleKeyDown(e: React.KeyboardEvent) {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      handleClick();
    }
  }

  const itemClasses = [
    styles.nodeItem,
    !node.isValid ? styles.invalid : '',
    node.nodeType === 'ignored' ? styles.ignored : '',
    isSelected ? styles.selected : '',
    isEvent ? styles.event : '',
  ]
    .filter(Boolean)
    .join(' ');

  const labelClasses = [
    styles.nodeLabel,
    hasChildren || isEvent ? styles.clickable : '',
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <li
      className={itemClasses}
      role={isEvent ? 'treeitem' : 'treeitem'}
      aria-expanded={hasChildren ? expanded : undefined}
      aria-selected={isEvent ? isSelected : undefined}
    >
      <span
        className={labelClasses}
        onClick={handleClick}
        onKeyDown={handleKeyDown}
        tabIndex={hasChildren || isEvent ? 0 : -1}
        style={{ paddingLeft: `${depth * 1.25}rem` }}
      >
        {hasChildren && (
          <span className={styles.toggle} aria-hidden>
            {expanded ? '▾' : '▸'}
          </span>
        )}
        {!hasChildren && <span className={styles.togglePlaceholder} aria-hidden />}
        <span className={styles.nodeName}>{node.name}</span>
        {!node.isValid && node.nodeType !== 'ignored' && (
          <span className={styles.badge} title="Ungültig">!</span>
        )}
      </span>

      {hasChildren && expanded && (
        <ul className={styles.subtree} role="group">
          {node.children!.map((child) => (
            <TreeNode
              key={child.path}
              node={child}
              selectedPath={selectedPath}
              onSelectEvent={onSelectEvent}
              expandedPaths={expandedPaths}
              onToggle={onToggle}
              depth={depth + 1}
            />
          ))}
        </ul>
      )}
    </li>
  );
}
