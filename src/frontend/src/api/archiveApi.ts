import type {
  AuthStatusDto,
  EventDetailDto,
  LoginRequestDto,
  RenameRequestDto,
  TreeNodeDto,
} from '../types/archive';

// ── Error types ────────────────────────────────────────────────────────────────

export class ApiError extends Error {
  readonly status: number;
  constructor(status: number, message: string) {
    super(message);
    this.status = status;
    this.name = 'ApiError';
  }
}

export class ConflictError extends ApiError {
  constructor(message: string) {
    super(409, message);
    this.name = 'ConflictError';
  }
}

export class UnauthorizedError extends ApiError {
  constructor() {
    super(401, 'Unauthorized');
    this.name = 'UnauthorizedError';
  }
}

// ── Fetch helper ──────────────────────────────────────────────────────────────

async function apiFetch<T>(
  path: string,
  options?: RequestInit,
): Promise<T> {
  const res = await fetch(path, {
    credentials: 'same-origin',
    headers: {
      'Content-Type': 'application/json',
      ...options?.headers,
    },
    ...options,
  });

  if (res.status === 401) {
    throw new UnauthorizedError();
  }

  if (res.status === 409) {
    let message = 'A rename operation is already in progress.';
    try {
      const body = await res.json() as { message?: string };
      if (body.message) message = body.message;
    } catch {
      // ignore parse errors
    }
    throw new ConflictError(message);
  }

  if (!res.ok) {
    let message = `Request failed with status ${res.status}`;
    try {
      const body = await res.json() as { message?: string };
      if (body.message) message = body.message;
    } catch {
      try {
        const text = await res.text();
        if (text) message = text;
      } catch {
        // ignore
      }
    }
    throw new ApiError(res.status, message);
  }

  // 204 No Content, or 200 with an empty body (e.g. login/logout returning Ok())
  if (res.status === 204) {
    return undefined as unknown as T;
  }

  const contentType = res.headers.get('content-type');
  const contentLength = res.headers.get('content-length');
  const hasBody =
    contentLength !== '0' &&
    contentType !== null &&
    contentType.includes('application/json');

  if (!hasBody) {
    return undefined as unknown as T;
  }

  return res.json() as Promise<T>;
}

// ── Auth ──────────────────────────────────────────────────────────────────────

export async function getAuthStatus(): Promise<AuthStatusDto> {
  return apiFetch<AuthStatusDto>('/api/auth/status');
}

export async function login(credentials: LoginRequestDto): Promise<void> {
  await apiFetch<void>('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify(credentials),
  });
}

export async function logout(): Promise<void> {
  await apiFetch<void>('/api/auth/logout', { method: 'POST' });
}

// ── Archive tree ──────────────────────────────────────────────────────────────

export async function getTree(path?: string): Promise<TreeNodeDto[]> {
  const url = path
    ? `/api/archive/tree?path=${encodeURIComponent(path)}`
    : '/api/archive/tree';
  return apiFetch<TreeNodeDto[]>(url);
}

// ── Events ────────────────────────────────────────────────────────────────────

export async function getEvent(path: string): Promise<EventDetailDto> {
  return apiFetch<EventDetailDto>(`/api/events/${encodeURIComponent(path)}`);
}

export async function renameEvent(
  path: string,
  request: RenameRequestDto,
): Promise<EventDetailDto> {
  return apiFetch<EventDetailDto>(
    `/api/events/rename?path=${encodeURIComponent(path)}`,
    {
      method: 'POST',
      body: JSON.stringify(request),
    },
  );
}

export async function renameEventByDateTime(path: string): Promise<EventDetailDto> {
  return apiFetch<EventDetailDto>(
    `/api/events/rename-by-datetime?path=${encodeURIComponent(path)}`,
    { method: 'POST' },
  );
}
