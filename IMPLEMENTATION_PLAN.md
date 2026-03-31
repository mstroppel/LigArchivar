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
- A lone `.dng` without companion `.jpg` is marked "lonely" and deleted on rename.

### Rename Operations

- **Sequential rename**: Renumbers files starting from a given number (`001`, `002`, …).
- **DateTime rename**: Renames files based on `LastWriteTimeUtc`.
- Both operations delete lonely `.dng` files and throw `RenameException` on conflicts.

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

- Replace `PropertyChangedBase` in `AppSettings` with a plain implementation of
  `INotifyPropertyChanged` or remove change notification entirely (settings are loaded
  once, not live-bound in a web context).
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
- Use `Task.Run` for CPU-bound directory enumeration if `System.IO.Abstractions` does not
  offer async directory listing (it doesn't as of v13 — consider upgrading to latest).

### 2.4 Update target framework

- Retarget `FL.LigArchivar.Core` from `netstandard2.0` to `net10.0`.
- Retarget tests from `netcoreapp2.1` to `net10.0`.
- Update `System.IO.Abstractions` to the latest version.
- Update NUnit to v4.x.

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

### 3.2 Frontend: React.js vs Next.js

| Aspect | React.js (Vite + React) | Next.js |
|---|---|---|
| **What it is** | Client-side SPA library | Full-stack React framework (SSR, SSG, API routes) |
| **Rendering** | Client-side only (SPA) | Server-side, static, or client — per page |
| **Hosting** | Static files served by the .NET backend or nginx | Requires its own Node.js server process |
| **Docker complexity** | Simple: build once, serve static files from the .NET container | Needs a second container or Node.js process alongside .NET |
| **SEO** | Not relevant (internal tool) | Not relevant (internal tool) |
| **API layer** | Calls the .NET API via fetch | Could use its own API routes — but that duplicates the .NET API, adding confusion |
| **Bundle size** | Small, fast to load | Larger framework overhead |
| **Learning curve** | Lower — just React + Vite | Higher — file-based routing, server components, middleware |
| **State management** | Full control (React Query, Zustand, etc.) | Same options, but SSR adds hydration complexity |

**Recommendation: React.js with Vite.**

Reasons:

- This is an internal tool with a single-page UI (tree view + file list + actions). There
  is no need for SSR, SSG, or file-based routing.
- Next.js would add a second runtime (Node.js) to the Docker container, increasing
  complexity for no benefit.
- With Vite + React, the frontend builds to static files that can be served directly by
  the ASP.NET backend (via `UseStaticFiles` or a reverse proxy), keeping the deployment to
  a single container.
- Next.js API routes would create an awkward overlap with the .NET API — either you
  duplicate logic or proxy everything through, which is pointless.

**Frontend tooling:**

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
│   │   ├── FL.LigArchivar.sln
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
(no token management needed on the client).

### 5.2 Concurrency

Multiple users may browse the archive simultaneously (read-only operations are safe).
Write operations (rename, delete) are serialized with a global `SemaphoreSlim(1,1)` in
the `ArchiveService`. If a rename is already in progress, a second request waits (or
returns `409 Conflict` with a message like "A rename operation is already in progress").

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
     Body: { "startNumber": 1 }
     Response: EventDetailDto (refreshed file list)

POST /api/events/{path}/rename-by-datetime
     DateTime-based rename. Acquires write lock.
     Response: EventDetailDto

POST /api/events/{path}/sort
     Body: { "sortBy": "name" | "date" }
     Response: EventDetailDto (re-sorted file list)
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
    bool IsLonely
);

public record RenameRequestDto(int StartNumber);
```

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
      - ARCHIVE_ROOT=/archive
      - AUTH_USERNAME=admin
      - AUTH_PASSWORD=changeme
```

### 6.4 Volume Mounting

The archive is mounted at `/archive` inside the container. The `ARCHIVE_ROOT` environment
variable tells the API where to find it. The mount must be read-write (`:rw`) since the
application renames and deletes files.

---

## 7. Implementation Phases

### Phase 1: Refactor Core (estimated: 3–4 days)

| # | Task | Details |
|---|---|---|
| 1.1 | Remove Caliburn.Micro from Core | Replace `PropertyChangedBase`, replace logging with `ILogger<T>` |
| 1.2 | Replace `FileSystemProvider` static with DI | Constructor-inject `IFileSystem` everywhere |
| 1.3 | Retarget to .NET 10 | Update `.csproj` files, update NuGet packages |
| 1.4 | Add async support | Make `LoadChildren`, `Rename`, `RenameToFileDateTime`, `TryCreate` async |
| 1.5 | Update tests | Retarget to .NET 10, update NUnit, fix tests after refactoring |
| 1.6 | Verify all tests pass | Green test suite before proceeding |

### Phase 2: Build API (estimated: 4–5 days)

| # | Task | Details |
|---|---|---|
| 2.1 | Create `FL.LigArchivar.Api` project | ASP.NET Core Web API, .NET 10, reference Core |
| 2.2 | Implement authentication | Cookie-based auth, credentials from env vars (`AUTH_USERNAME`, `AUTH_PASSWORD`), login/logout/status endpoints |
| 2.3 | Implement `ArchiveService` | Thin wrapper: creates `ArchiveRoot`, caches tree, maps to DTOs, holds `SemaphoreSlim` for write operations |
| 2.4 | Implement `ArchiveController` | `GET /api/archive/tree` |
| 2.5 | Implement `EventsController` | `GET`, `POST rename`, `POST rename-by-datetime`, `POST sort` |
| 2.6 | Add write-operation locking | `SemaphoreSlim(1,1)` in `ArchiveService`; return `409 Conflict` if lock is not available |
| 2.7 | Add path validation middleware | Prevent directory traversal attacks (e.g. `../../etc/passwd`) |
| 2.8 | Add configuration | `ARCHIVE_ROOT`, `AUTH_USERNAME`, `AUTH_PASSWORD` from environment, `appsettings.json` for defaults |
| 2.9 | Add API tests | Integration tests with `MockFileSystem` |

### Phase 3: Build Frontend (estimated: 4–5 days)

| # | Task | Details |
|---|---|---|
| 3.1 | Scaffold Vite + React + TypeScript project | `npm create vite@latest` |
| 3.2 | Define TypeScript types | Mirror the API DTOs |
| 3.3 | Implement API client | Fetch wrapper with error handling, auth cookie handled automatically by browser |
| 3.4 | Implement login page | Simple username/password form, redirect to main view on success |
| 3.5 | Implement archive tree view | Collapsible tree, color-coded validity (red/black) |
| 3.6 | Implement file list view | Table/grid showing files for the selected event |
| 3.7 | Implement rename controls | Start number input, rename button, rename-by-datetime button |
| 3.8 | Implement sort controls | Sort by name / sort by date |
| 3.9 | Error handling | Display rename errors, 409 conflict ("rename in progress"), connection errors |
| 3.10 | Styling | Clean, functional UI — match the existing WPF layout roughly |

### Phase 4: Docker & Deployment (estimated: 2 days)

| # | Task | Details |
|---|---|---|
| 4.1 | Create Dockerfile | Multi-stage build as described above |
| 4.2 | Create docker-compose.yml | Volume mount, port mapping, environment variables |
| 4.3 | Configure static file serving | ASP.NET serves `wwwroot` (frontend build output) |
| 4.4 | Add SPA fallback routing | `app.MapFallbackToFile("index.html")` for client-side routing |
| 4.5 | Test end-to-end in container | Build image, run with real archive mount, verify all features |

### Phase 5: Polish & Hardening (estimated: 2 days)

| # | Task | Details |
|---|---|---|
| 5.1 | Logging | Structured logging with `Serilog` or built-in `ILogger` |
| 5.2 | Health check endpoint | `GET /health` for Docker health checks |
| 5.3 | Security review | File path validation, container runs as non-root user |
| 5.4 | Performance | Consider lazy-loading subtrees if the archive is large |
| 5.5 | Documentation | README with build and deployment instructions |

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
