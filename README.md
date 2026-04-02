# FL.LigArchivar - Web Edition

Web-based archive checker and manager for organizing photo/video/audio assets in a structured hierarchy.

## Overview

This project is a web-based implementation of the FL.LigArchivar desktop application, migrated to run in a Docker container. The application allows checking file folder structures, validating naming conventions, and renaming files within correctly structured archives.

## Project Structure

```
FL.LigArchivar/
├── src/
│   ├── backend/
│   │   ├── FL.LigArchivar.Core/     # Domain logic (refactored from original)
│   │   ├── FL.LigArchivar.Api/      # ASP.NET Core Web API
│   └── frontend/                    # React.js web application
├── docker-compose.yml
└── Dockerfile
```

## Features

- Web-based UI for archive inspection
- File structure validation against naming conventions
- File renaming capabilities with sequential numbering or by datetime
- Support for multiple asset types (Photos, Audio, Video) with consistent directory structure
- Concurrent access protection

## Architecture

### Backend (src/backend)
- **FL.LigArchivar.Core**: Contains domain logic and models
- **FL.LigArchivar.Api**: ASP.NET Core Web API with authentication and endpoints

### Frontend (src/frontend)
- React.js with Vite build tool
- TypeScript for type safety
- React Query for server state management
- Tailwind CSS for styling

## How It Works

1. The Web API is deployed in a Docker container with the archive mounted as a volume
2. Users access the web application through their browser
3. The application validates that folder structures follow a specific hierarchy:
   ```
   ArchiveRoot
     └── AssetDirectory ("Digitalfoto" | "Ton" | "Video")
           └── YearDirectory (1700–3000)
                 └── ClubDirectory (17 predefined names)
                       └── EventDirectory (pattern: {ClubChar}_{Year}-{Month}-{Day}_{EventName})
                             └── Files (pattern: {ClubChar}_{Year}-{Month}-{Day}_{Number})
   ```

## Deployment

1. Build the Docker container:
   ```
   docker build -t fl-ligarchivar .
   ```

2. Run with volume mount and user credentials:
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
         - AUTH_PASSWORD=yourpassword
   ```

## API Endpoints

- `GET /api/archive/tree` - Returns the full archive tree structure
- `GET /api/events/{path}` - Returns event details including file list  
- `POST /api/events/{path}/rename` - Sequential rename operation
- `POST /api/events/{path}/rename-by-datetime` - Rename by datetime operation
- `POST /api/events/{path}/sort` - Sort files by name or date

## Development

### Backend
- Built with .NET 10
- ASP.NET Core Web API
- Uses System.IO.Abstractions for file operations
- Authentication via cookies
- Concurrency protection for write operations

### Frontend
- React.js with Vite
- TypeScript
- React Query for state management
- Tailwind CSS for styling
- Tree view component for archive navigation
- File list grid for event files

## Implementation Plan Status

This implementation follows the complete plan outlined in IMPLEMENTATION_PLAN.md:
1. Core logic refactored to .NET 10, removing Caliburn.Micro dependency
2. Static FileSystemProvider replaced with DI
3. Added async support
4. Authentication with username/password
5. Concurrency protection with write locks
6. ASP.NET Core Web API with controllers
7. React.js frontend with Vite build process
8. Docker containerization with volume mounting for archive access