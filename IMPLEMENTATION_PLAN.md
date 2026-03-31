# Implementation Plan: Archive Checker in Docker Container

## Overview
This document outlines the implementation plan for migrating the existing archive checking software to run in a Docker container with a .NET backend and web-based frontend. The solution will allow checking file and folder structures of an archive and renaming files in correctly structured folders.

## Existing Code Analysis
The current application is a WPF desktop application with:
- Core archive logic in `FL.LigArchivar.Core` project
- UI components in `FL.LigArchivar` project
- File system operations using DirectoryEx and FileSystemProvider
- Uses .NET Framework 4.7.2

## Refactoring Considerations
Before implementation, the existing code should be refactored to:
1. **Extract Core Logic**: Move archive checking/validation logic to a separate class library
2. **Separate Concerns**: Create clear boundaries between UI, business logic, and data access
3. **Dependency Injection**: Implement DI for better testability and maintainability
4. **Modernize APIs**: Update to .NET 6.0/8.0 with async/await patterns
5. **File System Abstraction**: Create interfaces for file system operations to support containerized environment

## Architecture

### Technology Stack
- **Backend**: .NET 6.0 or 8.0 (Web API)
- **Frontend**: ASP.NET Core Blazor WebAssembly or React.js
- **Containerization**: Docker
- **File System Access**: .NET FileSystem APIs with Docker volume mounting
- **Networking**: HTTP/HTTPS for web interface

### Components
1. **API Layer** - RESTful services for file operations
2. **Business Logic Layer** - Archive structure validation and file operations
3. **Data Layer** - File system access and data manipulation
4. **Web Interface** - Hosted web application for user interaction

## Implementation Steps

### 1. Refactoring Phase
- Create new class library for core archive logic
- Extract and refactor existing validation logic
- Implement dependency injection patterns
- Create interfaces for file system operations
- Update to async/await patterns

### 2. Project Structure Creation
```
ArchiveChecker/
├── src/
│   ├── ArchiveChecker.API/          # ASP.NET Core Web API
│   ├── ArchiveChecker.Core/         # Business logic and models
│   ├── ArchiveChecker.Web/          # Web frontend (Blazor or React)
│   └── ArchiveChecker.Infrastructure/ # File system access
├── docker-compose.yml
└── Dockerfile
```

### 3. Backend Implementation
- Migrate the core archive checking logic to .NET 6.0/8.0
- Create RESTful API endpoints for:
  - Getting directory structure
  - Validating folder/file structure
  - Renaming files
  - File operations
- Implement file system access using .NET FileSystem APIs

### 4. Frontend Implementation
- Create web-based UI with either:
  - Blazor WebAssembly for .NET-based frontend
  - React.js for JavaScript-based frontend
- Implement tree view for archive structure
- Add file management features (rename, validate)

### 5. Docker Setup
- Create Dockerfile for the backend
- Create docker-compose.yml for multi-container setup
- Configure volume mounting for the archive
- Set up networking between containers

### 6. Security Considerations
- Implement authentication/authorization
- Validate all file paths to prevent directory traversal
- Implement proper logging
- Run container with minimal privileges

## Expected Features
- Web-based UI for archive structure inspection
- File structure validation
- Renaming capabilities for files with correct structure
- Archive folder organization
- Docker container deployment

## Implementation Timeline
1. **Week 1**: Refactoring phase - extract core logic and prepare for web implementation
2. **Week 2**: Backend development - API creation and file system integration
3. **Week 3**: Frontend development - web interface implementation
4. **Week 4**: Docker container configuration and testing