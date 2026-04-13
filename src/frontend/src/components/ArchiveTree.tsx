import { useState } from 'react';
import type { TreeNodeDto } from '../types/archive';
import styles from './ArchiveTree.module.css';

interface ArchiveTreeProps {
  nodes: TreeNodeDto[];
  selectedPath: string | null;
  onSelectEvent: (path: string) => void;
}

export function ArchiveTree({ nodes, selectedPath, onSelectEvent }: ArchiveTreeProps) {
  return (
    <ul className={styles.tree} role="tree">
      {nodes.map((node) => (
        <TreeNode
          key={node.path}
          node={node}
          selectedPath={selectedPath}
          onSelectEvent={onSelectEvent}
          depth={0}
        />
      ))}
    </ul>
  );
}

interface TreeNodeProps {
  node: TreeNodeDto;
  selectedPath: string | null;
  onSelectEvent: (path: string) => void;
  depth: number;
}

function TreeNode({ node, selectedPath, onSelectEvent, depth }: TreeNodeProps) {
  // Expand asset/year/club nodes by default; events and invalid/ignored collapsed
  const isLeafLike = node.nodeType === 'event' || node.nodeType === 'invalid' || node.nodeType === 'ignored';
  const [expanded, setExpanded] = useState(!isLeafLike && depth <= 1);

  const hasChildren = node.children && node.children.length > 0;
  const isSelected = node.path === selectedPath;
  const isEvent = node.nodeType === 'event';

  function handleClick() {
    if (isEvent) {
      onSelectEvent(node.path);
    } else if (hasChildren) {
      setExpanded((prev) => !prev);
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
              depth={depth + 1}
            />
          ))}
        </ul>
      )}
    </li>
  );
}
