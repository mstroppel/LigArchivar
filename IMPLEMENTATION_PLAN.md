# Implementation Plan: FL.LigArchivar Web

## Overview

Migrate the existing WPF desktop application (FL.LigArchivar) to a web-based solution
running in a Docker container on the server. The archive directory is mounted into the
container as a volume. Users access the application via a web browser.

---

## 1. Existing Code Analysis

The current application is a WPF desktop app (.NET Framework 4.7.2) using Caliburn.Micro
for MVVM. It connects to the archive via a network share (e.g. `\\sepp\archiv\`).

### Current Project Structure

| Project | Target | Purpose |
|---|---|---|
| `FL.LigArchivar` | net472 (WPF) | Desktop UI with TreeView, DataGrid, rename controls |
| `FL.LigArchivar.Core` | netstandard2.0 | Domain logic: archive structure validation, rename |
| `FL.LigArchivar.Core.Tests` | netcoreapp2.1 | NUnit tests with MockFileSystem |

### Domain Model (5-level hierarchy)

```
ArchiveRoot
  └── AssetDirectory          ("Digitalfoto" | "Ton" | "Video")
        ├── IgnoredFileSystemItem  ("Ordnerstruktur" — skipped)
        └── YearDirectory     (1700–3000)
              └── ClubDirectory   (17 predefined names, e.g. "A-Albverein")
                    └── EventDirectory  (pattern: {ClubChar}_{Year}-{Month}-{Day}_{EventName})
                          └── DataFiles / DataFile  (pattern: {ClubChar}_{Year}-{Month}-{Day}_{Number})
```

### Key Validation Rules

- Each hierarchy level validates its directory name against strict patterns/whitelists.
- Cross-validation: ClubChar and Year in child names must match parent directories.
- Validity propagates upward: one invalid child makes all ancestors invalid.
- Files named `Thumbs.db` are ignored.
- A `.dng` without a companion `.jpg` is marked "orphaned" and deleted on rename.

### Rename Operations

- **Sequential rename**: Renumbers files starting from a given number (`001`, `002`, …).
- **DateTime rename**: Renames files based on `LastWriteTimeUtc`.
- Both operations delete orphaned `.dng` files and throw `RenameException` on conflicts.

### Dependencies Worth Noting

- `System.IO.Abstractions` (v13) — used throughout Core for testable file I/O via
  `FileSystemProvider.Instance`. This is a good pattern to keep.
- `Caliburn.Micro.Core` — used in Core for `PropertyChangedBase` and `ILog`. This is a
  WPF/MVVM concern that has leaked into the domain layer.

---

## 2. Refactoring the Existing Core (Before Starting the Web App)

The existing `FL.LigArchivar.Core` is mostly well-structured but has several issues that
should be addressed before building the web layer on top of it.

### 2.1 Remove Caliburn.Micro dependency from Core

`Caliburn.Micro.Core` is referenced in the Core project for two things:

- `PropertyChangedBase` (used by `AppSettings`)
- `ILog` / `LogManager` (used in `FileSystemItemBase` and `EventDirectory`)

**Actions:**

- Remove `PropertyChangedBase` from `AppSettings` entirely — change notification is not
  needed in a web context where settings are loaded once at startup, not live-bound to UI.
- Replace `Caliburn.Micro` logging with `Microsoft.Extensions.Logging.ILogger<T>` injected
  via DI.
- Remove the `Caliburn.Micro.Core` NuGet reference from the Core project.

### 2.2 Replace static FileSystemProvider with DI

`FileSystemProvider.Instance` is a static mutable singleton — a global variable. This
works for the current test setup but is problematic for a web server handling concurrent
requests.

**Actions:**

- Inject `IFileSystem` via constructor into all classes that need it (`ArchiveRoot`,
  `DirectoryEx`, `DataFiles`, `DataFile`, `XmlSerializerEx`).
- Remove `FileSystemProvider` entirely.
- Register `IFileSystem` as a singleton in the DI container (one real filesystem instance
  is fine).

### 2.3 Introduce async file operations

File I/O over a mounted volume can be slow. The current code is entirely synchronous,
which would block ASP.NET request threads.

**Actions:**

- Make `EventDirectory.LoadChildren()`, `Rename()`, `RenameToFileDateTime()` async.
- Make `ArchiveRoot.TryCreate()` async.
- Update `System.IO.Abstractions` to the latest version (v21+), which provides async
  file and directory operations. Use async methods where available. For any remaining
  synchronous I/O (e.g. directory enumeration), accept the blocking call rather than
  wrapping in `Task.Run` — the mounted volume is effectively local I/O and `Task.Run`
  on ASP.NET Core does not save thread pool resources.

### 2.4 Update target framework

- Retarget `FL.LigArchivar.Core` from `netstandard2.0` to `net10.0`.
- Retarget tests from `netcoreapp2.1` to `net10.0`.
- Update `System.IO.Abstractions` to the latest version.
- Migrate from NUnit to xUnit v3.

### 2.5 Improve error handling

- The current code catches broad exception types in `DirectoryEx.Exists`. This is
  acceptable for robustness but should be reviewed.
- `RenameException` carries German user-facing messages. Consider separating error codes
  from display messages for i18n in the future (not blocking for MVP).

---

## 3. Technology Decisions

### 3.1 Backend: ASP.NET Core Web API on .NET 10

Straightforward choice. Minimal API or controller-based — controller-based is recommended
here since the API surface is well-defined and grouping by resource (archive tree, events,
files) maps naturally to controllers.

### 3.2 Frontend: React.js with Vite

Client-side SPA. Builds to static files served directly by the ASP.NET backend
(via `UseStaticFiles`), keeping deployment to a single container.

**Tooling:**

- **Vite** — build tool and dev server
- **React 19** — UI library
- **TypeScript** — type safety
- **React Query (TanStack Query)** — server state management, caching, auto-refresh
- **A tree view component** — e.g. `react-arborist` or a custom component
- **Tailwind CSS** or **CSS Modules** — styling (lightweight, no component library needed
  for this UI)

---

## 4. Project Structure

```
FL.LigArchivar/
├── src/
│   ├── backend/
│   │   ├── FL.LigArchivar.slnx
│   │   ├── FL.LigArchivar.Core/           # Domain logic (retargeted to net10.0)
│   │   │   ├── FL.LigArchivar.Core.csproj
│   │   │   ├── ArchiveRoot.cs
│   │   │   ├── Data/
│   │   │   │   ├── IFileSystemItem.cs
│   │   │   │   ├── IFileSystemItemWithChildren.cs
│   │   │   │   ├── FileSystemItemBase.cs
│   │   │   │   ├── AssetDirectory.cs
│   │   │   │   ├── YearDirectory.cs
│   │   │   │   ├── ClubDirectory.cs
│   │   │   │   ├── EventDirectory.cs
│   │   │   │   ├── DataFiles.cs
│   │   │   │   ├── DataFile.cs
│   │   │   │   └── ...
│   │   │   └── Utilities/
│   │   │       ├── Patterns.cs
│   │   │       └── ...
│   │   ├── FL.LigArchivar.Api/            # ASP.NET Core Web API
│   │   │   ├── FL.LigArchivar.Api.csproj
│   │   │   ├── Program.cs
│   │   │   ├── Controllers/
│   │   │   │   ├── ArchiveController.cs
│   │   │   │   ├── EventsController.cs
│   │   │   │   └── FilesController.cs
│   │   │   ├── Models/                    # API DTOs
│   │   │   │   ├── TreeNodeDto.cs
│   │   │   │   ├── EventDetailDto.cs
│   │   │   │   ├── FileGroupDto.cs
│   │   │   │   └── RenameRequestDto.cs
│   │   │   └── Services/                  # Thin service layer bridging API ↔ Core
│   │   │       └── ArchiveService.cs
│   │   └── FL.LigArchivar.Core.Tests/
│   │       ├── FL.LigArchivar.Core.Tests.csproj
│   │       └── ...
│   └── frontend/
│       ├── package.json
│       ├── vite.config.ts
│       ├── tsconfig.json
│       ├── index.html
│       └── src/
│           ├── main.tsx
│           ├── App.tsx
│           ├── api/                       # API client functions
│           │   └── archiveApi.ts
│           ├── components/
│           │   ├── ArchiveTree.tsx
│           │   ├── FileList.tsx
│           │   ├── RenameControls.tsx
│           │   └── ...
│           ├── hooks/
│           │   └── useArchive.ts
│           └── types/
│               └── archive.ts
├── Dockerfile
├── docker-compose.yml
└── .dockerignore
```

---

## 5. API Design

### 5.1 Authentication

Simple cookie-based authentication with a single username/password pair configured via
environment variables (`AUTH_USERNAME`, `AUTH_PASSWORD`).

```
POST /api/auth/login
     Body: { "username": "...", "password": "..." }
     Response: 200 OK (sets auth cookie) or 401 Unauthorized

POST /api/auth/logout
     Response: 200 OK (clears auth cookie)

GET  /api/auth/status
     Response: { "authenticated": true/false }
```

All other `/api/*` endpoints require a valid auth cookie — unauthenticated requests
return `401`. The frontend shows a login form and stores the session via the cookie
(no token management needed on the client). The auth cookie is configured with `HttpOnly`,
`Secure`, and `SameSite=Strict`. In production, a reverse proxy (e.g. Caddy, nginx,
Traefik) handles TLS termination in front of the container.

### 5.2 Concurrency

Multiple users may browse the archive simultaneously (read-only operations are safe).
Write operations (rename, delete) are serialized with a global `SemaphoreSlim(1,1)` in
the `ArchiveService`. If a rename is already in progress, a second request immediately
returns `409 Conflict` with a message like "A rename operation is already in progress".

### 5.3 Endpoints

```
GET  /api/archive/tree
     Returns the full archive tree structure with validity flags.
     Response: TreeNodeDto[] (recursive)

GET  /api/archive/tree?path=Digitalfoto/2018
     Returns a subtree starting from the given path (for lazy loading if
     the archive is large).

GET  /api/events/{path}
     Returns event details including file list.
     path = URL-encoded relative path, e.g. "Digitalfoto/2018/A-Albverein/A_2018-05-01_Maiwanderung"
     Response: EventDetailDto

POST /api/events/{path}/rename
     Sequential rename. Acquires write lock.
     Body: { "startNumber": 1, "fileOrder": ["A_2018-05-01_003", "A_2018-05-01_001"] }
     fileOrder is optional. If omitted, the backend uses its natural (filesystem) order.
     If provided, files are renamed in the given order — enabling any client-side sorting
     or custom drag-and-drop reordering without additional API changes.
     Response: EventDetailDto (refreshed file list)

POST /api/events/{path}/rename-by-datetime
     DateTime-based rename. Acquires write lock.
     Response: EventDetailDto
```

### 5.4 DTO Examples

```csharp
public record TreeNodeDto(
    string Name,
    string Path,          // relative path from archive root
    bool IsValid,
    string NodeType,      // "asset", "year", "club", "event", "invalid", "ignored"
    TreeNodeDto[]? Children
);

public record EventDetailDto(
    string Name,
    string Path,
    string FilePrefix,    // e.g. "A_2018-05-01_"
    bool IsValid,
    bool IsInPictures,
    FileGroupDto[] Files
);

public record FileGroupDto(
    string Name,
    string[] Extensions,
    string[] Properties,
    bool IsValid,
    bool IsOrphaned,
    DateTime LastWriteTimeUtc   // used by the frontend for sort-by-date
);

public record RenameRequestDto(int StartNumber, string[]? FileOrder = null);
```

`FileOrder` contains file base names (without extensions) in the desired rename sequence.
The backend looks up each name in the loaded `Children` list and renames in that order.
Omitting `FileOrder` falls back to natural filesystem order, preserving backward compatibility.

---

## 6. Docker Setup

### 6.1 Single-container approach

The frontend builds to static files. The ASP.NET backend serves them via
`UseStaticFiles`. This keeps deployment simple: one container, one process.

### 6.2 Dockerfile (multi-stage build)

```dockerfile
# Stage 1: Build frontend
FROM node:22-alpine AS frontend-build
WORKDIR /app/frontend
COPY src/frontend/package*.json ./
RUN npm ci
COPY src/frontend/ ./
RUN npm run build

# Stage 2: Build backend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /app
COPY src/backend/ ./
RUN dotnet publish FL.LigArchivar.Api/FL.LigArchivar.Api.csproj \
    -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=backend-build /app/publish ./
COPY --from=frontend-build /app/frontend/dist ./wwwroot
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "FL.LigArchivar.Api.dll"]
```

### 6.3 docker-compose.yml

```yaml
services:
  ligarchivar:
    build: .
    ports:
      - "8080:8080"
    volumes:
      - /path/to/archive:/archive:rw
    environment:
      - AUTH_USERNAME=admin
      - AUTH_PASSWORD=changeme
```

In production, credentials should be provided via a `.env` file (excluded from version
control) or Docker secrets, not hardcoded in `docker-compose.yml`.

### 6.4 Volume Mounting

The archive root path defaults to `/archive` and is configurable via the `ARCHIVE_ROOT`
environment variable (or `appsettings.json`). The mount must be read-write (`:rw`) since the
application renames and deletes files.

---

## 7. Implementation Phases

### Phase 1: Refactor Core (estimated: 3–4 days)

- [x] **1.1** Retarget to .NET 10 — Update `.csproj` files, update NuGet packages; use Central Package Management (`Directory.Packages.props`) to manage NuGet package versions consistently across all projects
- [x] **1.2** Remove Caliburn.Micro from Core — Replace `PropertyChangedBase`, replace logging with `ILogger<T>`
- [x] **1.3** Replace `FileSystemProvider` static with DI — Constructor-inject `IFileSystem` everywhere
- [x] **1.4** Add async support — Make `LoadChildren`, `Rename`, `RenameToFileDateTime`, `TryCreate` async; remove `SortByName` and `SortByDate` from `EventDirectory` (sorting is now pure frontend state)
- [x] **1.5** Update tests — Retarget to .NET 10, migrate from NUnit to xUnit v3, fix tests after refactoring
- [x] **1.6** Verify all tests pass — Green test suite before proceeding

### Phase 2: Build API (estimated: 4–5 days)

- [x] **2.1** Create `FL.LigArchivar.Api` project — ASP.NET Core Web API, .NET 10, reference Core; use `.slnx` format for the solution file
- [x] **2.2** Implement authentication — Cookie-based auth, credentials from env vars (`AUTH_USERNAME`, `AUTH_PASSWORD`), login/logout/status endpoints
- [x] **2.3** Implement `ArchiveService` — Thin wrapper: creates `ArchiveRoot`, caches tree, maps to DTOs, holds `SemaphoreSlim` for write operations
- [x] **2.4** Implement `ArchiveController` — `GET /api/archive/tree`
- [x] **2.5** Implement `EventsController` — `GET`, `POST rename` (with optional `fileOrder`), `POST rename-by-datetime`
- [x] **2.6** Add write-operation locking — `SemaphoreSlim(1,1)` in `ArchiveService`; return `409 Conflict` if lock is not available
- [x] **2.7** Add path validation middleware — Prevent directory traversal attacks (e.g. `../../etc/passwd`)
- [x] **2.8** Add configuration — `AUTH_USERNAME`, `AUTH_PASSWORD`, `ARCHIVE_ROOT` from environment, `appsettings.json` for defaults
- [x] **2.9** Add API tests — Integration tests with `MockFileSystem`

### Phase 3: Build Frontend (estimated: 4–5 days)

- [x] **3.1** Scaffold Vite + React + TypeScript project — `npm create vite@latest`; configure
  Vite's dev server to proxy `/api` requests to the ASP.NET backend (e.g. `http://localhost:5000`)
  so that cookies and API calls work during local development without CORS configuration
- [x] **3.2** Define TypeScript types — Mirror the API DTOs
- [x] **3.3** Implement API client — Fetch wrapper with error handling, auth cookie handled automatically by browser
- [x] **3.4** Implement login page — Simple username/password form, redirect to main view on success
- [x] **3.5** Implement archive tree view — Collapsible tree, color-coded validity (red/black)
- [x] **3.6** Implement file list view — Table/grid showing files for the selected event
- [x] **3.7** Implement rename controls — Start number input, rename button, rename-by-datetime button; pass current file order from FE state to the rename request
- [x] **3.8** Implement sort controls — Sort by name / sort by date as pure client-side state using `LastWriteTimeUtc` from `FileGroupDto`; sorted order is sent via `fileOrder` on rename
- [x] **3.9** Error handling — Display rename errors, 409 conflict ("rename in progress"), connection errors
- [x] **3.10** Styling — Clean, functional UI — match the existing WPF layout roughly

### Phase 4: Docker & Deployment (estimated: 2 days)

- [x] **4.1** Create Dockerfile — Multi-stage build as described above
- [x] **4.2** Create docker-compose.yml — Volume mount, port mapping, environment variables
- [x] **4.3** Configure static file serving — ASP.NET serves `wwwroot` (frontend build output)
- [x] **4.4** Add SPA fallback routing — `app.MapFallbackToFile("index.html")` for client-side routing
- [ ] **4.5** Test end-to-end in container — Build image, run with real archive mount, verify all features (must be done manually; Docker not available in CI environment)

### Phase 5: Polish & Hardening (estimated: 2 days)

- [x] **5.1** Logging — Structured logging with `Serilog` or built-in `ILogger`
- [x] **5.2** Health check endpoint — `GET /health` for Docker health checks
- [x] **5.3** Security review — File path validation, container runs as a configurable
  user/group so that it matches the owner of the mounted archive files on the host;
  `PUID` and `PGID` environment variables are read at container startup (e.g. via an
  entrypoint script) to set the effective UID/GID of the process, following the same
  pattern used by LinuxServer.io images
- [x] **5.4** Performance — Tree is loaded from disk on each request (fast for local/NFS
  mounts). Lazy-loading subtrees is deferred to a future iteration (see 9. Future Considerations).
- [x] **5.5** Documentation — README with build and deployment instructions

### Phase 6: CI/CD with GitHub Actions (estimated: 1–2 days)

- [x] **6.1** PR validation workflow — Triggered on pull requests to `main`; runs backend
  tests (`dotnet test`), frontend lint/type-check (`npm run lint`, `tsc --noEmit`), and
  frontend unit tests (`npm test`); reports results as a required status check
- [x] **6.2** Docker build & push workflow — Triggered on push to `main` and on creation
  of a new version tag (e.g. `v*`); builds the multi-stage Docker image and pushes it to
  the container registry (e.g. GitHub Container Registry `ghcr.io`); tags the image with
  both `latest` (for `main` pushes) and the version tag (e.g. `v1.2.3`) for releases

---

## 8. Decisions

- **Existing WPF app**: Remove completely. No parallel operation during migration.
  The old `FL.LigArchivar` (WPF) and its associated projects will be deleted or archived
  in a separate git branch before starting the new implementation.
- **Authentication**: Simple single-user auth. Username and password are configured via
  environment variables (`AUTH_USERNAME`, `AUTH_PASSWORD`) in the `docker-compose.yml`.
  Cookie-based session — no tokens, no external identity provider.
- **Concurrent access**: Multiple sessions may browse the archive simultaneously.
  Write operations (rename, delete) are serialized with a `SemaphoreSlim(1,1)`. If a
  write is already in progress, subsequent write requests return `409 Conflict`.

---

## 9. Future Considerations

- **File preview**: Show image thumbnails for the "Digitalfoto" asset type.
- **WebSocket / SignalR**: Push progress updates for long-running rename operations
  instead of waiting for the HTTP response to complete.
- **i18n**: The existing code has German error messages hardcoded. Consider separating
  error codes from display text if multi-language support becomes relevant.
