# FL.LigArchivar

A web-based archive management tool for the Ligachiv photo/audio/video archive.
Runs as a single Docker container; the archive directory is mounted as a volume.

## Features

- Browse the 5-level archive hierarchy (Asset → Year → Club → Event → Files)
- Validity color-coding — invalid paths are highlighted in red
- Sequential rename: renumber files starting from a given number
- Date-time rename: rename files using their `LastWriteTimeUtc`
- Sort files by name or by date before renaming
- Cookie-based authentication (single user, credentials via environment variables)
- Write operations are serialized — concurrent rename attempts return `409 Conflict`

---

## Quick start (Docker Compose)

1. Copy `docker-compose.yml` and create a `.env` file next to it:

  ```envfile
  AUTH_USERNAME=admin
  AUTH_PASSWORD=your-secure-password
  ARCHIVE_ROOT=/archive
  # Optional: match the UID/GID of the archive owner on the host
  # PUID=1000
  # PGID=1000
  ```

2. Edit `docker-compose.yml` to point the volume at your archive directory:

   ```yaml
   volumes:
     - /path/to/your/archive:/archive:rw
   ```

3. Start the container:

   ```sh
   docker compose up -d
   ```

4. Open `http://<server>:8080` in a browser and log in.

---

## Environment variables

| Variable        | Default    | Description                                             |
| --------------- | ---------- | ------------------------------------------------------- |
| `AUTH_USERNAME` | *required* | Login username                                          |
| `AUTH_PASSWORD` | *required* | Login password                                          |
| `ARCHIVE_ROOT`  | `/archive` | Absolute path inside the container to the archive root  |
| `PUID`          | (unset)    | UID of the user that owns the archive files on the host |
| `PGID`          | (unset)    | GID of the user that owns the archive files on the host |

Setting `PUID`/`PGID` causes the container process to run as that user via
[gosu](https://github.com/tianon/gosu), following the same pattern as
[LinuxServer.io](https://docs.linuxserver.io/general/understanding-puid-and-pgid/) images.

---

## Building from source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 24](https://nodejs.org/) + npm

### Backend

```sh
cd src/backend
dotnet build
dotnet test
```

### Frontend (dev server with hot reload)

```sh
cd src/frontend
npm install
npm run dev
```

The Vite dev server proxies `/api` and `/health` to `http://localhost:5000`
(the default ASP.NET dev port), so both can run side-by-side without CORS issues.

### Build Docker image

```sh
docker build -t ligarchivar .
```

---

## Project structure

```text
FL.LigArchivar/
├── src/
│   ├── backend/
│   │   ├── FL.LigArchivar.Core/        # Domain logic (.NET 10)
│   │   ├── FL.LigArchivar.Core.Tests/  # xUnit tests
│   │   ├── FL.LigArchivar.Api/         # ASP.NET Core Web API
│   │   └── FL.LigArchivar.Api.Tests/   # Integration tests
│   └── frontend/                       # React + TypeScript (Vite)
├── Dockerfile                          # Multi-stage build
├── docker-compose.yml
└── entrypoint.sh                       # PUID/PGID user-switching
```

---

## Tech stack

| Layer                       | Technology                       | Version       |
| --------------------------- | -------------------------------- | ------------- |
| **Backend language**        | C#                               | 13            |
| **Backend framework**       | ASP.NET Core Web API             | .NET 10       |
| **Frontend library**        | React                            | 19            |
| **Frontend language**       | TypeScript                       | 6             |
| **Frontend build tool**     | Vite                             | 8             |
| **Server state / caching**  | TanStack Query (React Query)     | 5             |
| **Backend testing**         | xUnit + FluentAssertions         | xUnit 2, FA 8 |
| **Integration testing**     | Microsoft.AspNetCore.Mvc.Testing | .NET 10       |
| **File-system abstraction** | System.IO.Abstractions           | 21            |
| **Containerization**        | Docker (multi-stage build)       | —             |
| **CI/CD**                   | GitHub Actions                   | —             |
| **API testing**             | Bruno                            | —             |

---

## Architecture notes

- **Single container**: the frontend is built to static files and served by the
  ASP.NET backend via `UseStaticFiles` + `MapFallbackToFile("index.html")`.
- **No database**: the archive directory on the volume is the source of truth.
  The tree is loaded from disk on every request (fast for local/NFS mounts).
- **Authentication**: `HttpOnly` + `SameSite=Strict` session cookie.
  Deploy behind a TLS-terminating reverse proxy (Caddy, nginx, Traefik) in production.
- **Write locking**: `SemaphoreSlim(1,1)` in `ArchiveService` serializes rename
  operations. Concurrent requests get `409 Conflict` immediately.

---

## API endpoints

| Method | Path                                      | Description                      |
| ------ | ----------------------------------------- | -------------------------------- |
| POST   | `/api/auth/login`                         | Log in (sets session cookie)     |
| POST   | `/api/auth/logout`                        | Log out                          |
| GET    | `/api/auth/status`                        | Check authentication status      |
| GET    | `/api/archive/tree`                       | Full archive tree                |
| GET    | `/api/archive/tree?path=...`              | Subtree at relative path         |
| GET    | `/api/events?path=...`                    | Event details + file list        |
| POST   | `/api/events/rename?path=...`             | Sequential rename                |
| POST   | `/api/events/rename-by-datetime?path=...` | Date-time rename                 |
| GET    | `/health`                                 | Health check (for Docker/probes) |
