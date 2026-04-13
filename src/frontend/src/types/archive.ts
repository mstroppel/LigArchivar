// Mirrors C# DTOs from FL.LigArchivar.Api/Models/Dtos.cs

export type NodeType = 'asset' | 'year' | 'club' | 'event' | 'invalid' | 'ignored';

export interface TreeNodeDto {
  name: string;
  path: string;
  isValid: boolean;
  nodeType: NodeType;
  children: TreeNodeDto[] | null;
}

export interface FileGroupDto {
  name: string;
  extensions: string[];
  properties: string[];
  isValid: boolean;
  isOrphaned: boolean;
  lastWriteTimeUtc: string; // ISO 8601 string from JSON
}

export interface EventDetailDto {
  name: string;
  path: string;
  filePrefix: string;
  isValid: boolean;
  isInPictures: boolean;
  files: FileGroupDto[];
}

export interface RenameRequestDto {
  startNumber: number;
  fileOrder?: string[];
}

export interface LoginRequestDto {
  username: string;
  password: string;
}

export interface AuthStatusDto {
  authenticated: boolean;
}
