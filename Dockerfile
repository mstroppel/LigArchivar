# ── Stage 1: Build frontend ───────────────────────────────────────────────────
FROM node:24-alpine AS frontend-build
WORKDIR /app/frontend
COPY src/frontend/package*.json ./
RUN npm ci
COPY src/frontend/ ./
RUN npm run build

# ── Stage 2: Build backend ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /app
COPY src/backend/ ./
RUN dotnet publish FL.LigArchivar.Api/FL.LigArchivar.Api.csproj \
    -c Release -o /app/publish

# ── Stage 3: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Install curl for the health check and gosu for user switching
RUN apt-get update && apt-get install -y --no-install-recommends curl gosu \
    && rm -rf /var/lib/apt/lists/*

COPY --from=backend-build /app/publish ./
# Copy frontend build output into wwwroot so ASP.NET serves it as static files
COPY --from=frontend-build /app/frontend/dist ./wwwroot

ENV ASPNETCORE_ENVIRONMENT=Production

# Entrypoint script: honour PUID/PGID environment variables so that the
# process runs as the same user/group that owns the mounted archive files.
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["/entrypoint.sh"]
