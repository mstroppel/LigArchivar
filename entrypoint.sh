#!/bin/sh
# ── entrypoint.sh ─────────────────────────────────────────────────────────────
# Optionally run as a specific UID/GID so the process can read/write the
# mounted archive files owned by that user on the host.
# Set PUID and PGID environment variables to match the archive owner's IDs.
# If not set, the container runs as root (default for the ASP.NET base image).
#
# This follows the same pattern used by linuxserver.io images.

PUID=${PUID:-}
PGID=${PGID:-}

if [ -n "$PUID" ] || [ -n "$PGID" ]; then
  TARGET_UID="${PUID:-0}"
  TARGET_GID="${PGID:-0}"

  echo "Running as UID=${TARGET_UID} GID=${TARGET_GID}"

  # Create group if the GID doesn't already exist
  if [ "${TARGET_GID}" != "0" ] && ! getent group "${TARGET_GID}" > /dev/null 2>&1; then
    groupadd -g "${TARGET_GID}" appgroup
  fi

  # Create user if the UID doesn't already exist
  if [ "${TARGET_UID}" != "0" ] && ! getent passwd "${TARGET_UID}" > /dev/null 2>&1; then
    GROUP_NAME=$(getent group "${TARGET_GID}" | cut -d: -f1)
    useradd -u "${TARGET_UID}" -g "${GROUP_NAME:-appgroup}" -M -s /sbin/nologin appuser
  fi

  USER_ENTRY=$(getent passwd "${TARGET_UID}")
  if [ -n "$USER_ENTRY" ]; then
    USER_NAME=$(echo "$USER_ENTRY" | cut -d: -f1)
    exec gosu "${USER_NAME}" dotnet FL.LigArchivar.Api.dll "$@"
  fi
fi

exec dotnet FL.LigArchivar.Api.dll "$@"
