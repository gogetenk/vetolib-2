// In development without an explicit API URL, use relative paths so MSW can intercept them.
// In production, NEXT_PUBLIC_API_URL should point to the real backend.
const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? "";

// Lazy import to avoid circular dependency — analytics imports from this module indirectly
function trackApiError(endpoint: string, status: number, errorCode?: string): void {
  import('@/lib/analytics').then(({ trackEvent, AnalyticsEvents }) => {
    trackEvent(AnalyticsEvents.API_ERROR_DISPLAYED, {
      endpoint,
      status_code: String(status),
      error_code: errorCode ?? "",
    })
  }).catch(() => {/* ignore tracking failures */})
}

function getAccessToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("access_token");
}

function setAccessToken(token: string): void {
  localStorage.setItem("access_token", token);
}

function clearTokens(): void {
  localStorage.removeItem("access_token");
  localStorage.removeItem("refresh_token");
}

async function refreshAccessToken(): Promise<boolean> {
  const refreshToken = localStorage.getItem("refresh_token");
  if (!refreshToken) return false;

  try {
    const res = await fetch(`${API_BASE}/api/v1/auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    });

    if (!res.ok) return false;

    const data = await res.json();
    setAccessToken(data.accessToken);
    if (data.refreshToken) {
      localStorage.setItem("refresh_token", data.refreshToken);
    }
    return true;
  } catch {
    return false;
  }
}

async function apiFetch<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const token = getAccessToken();
  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options.headers as Record<string, string>),
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  let res = await fetch(`${API_BASE}${path}`, { ...options, headers });

  // Auto-refresh on 401
  if (res.status === 401 && token) {
    const refreshed = await refreshAccessToken();
    if (refreshed) {
      headers["Authorization"] = `Bearer ${getAccessToken()}`;
      res = await fetch(`${API_BASE}${path}`, { ...options, headers });
    } else {
      clearTokens();
      import('@/lib/analytics').then(({ trackEvent, AnalyticsEvents }) => {
        trackEvent(AnalyticsEvents.SESSION_EXPIRED, {
          time_since_login: "",
        })
      }).catch(() => {/* ignore */})
      if (typeof window !== "undefined") {
        window.location.href = "/login";
      }
      throw new Error("Session expired");
    }
  }

  if (!res.ok) {
    const error = await res.json().catch(() => ({ title: "Request failed" }));
    const errorCode = typeof error === "object" && error !== null && "code" in error
      ? String((error as { code: unknown }).code)
      : undefined;
    trackApiError(path, res.status, errorCode);
    throw new ApiError(res.status, error);
  }

  // Handle 204 No Content
  if (res.status === 204) {
    return undefined as T;
  }

  return res.json();
}

export class ApiError extends Error {
  public status: number;
  public body: unknown;

  constructor(status: number, body: unknown) {
    super(
      typeof body === "object" && body !== null && "title" in body
        ? (body as { title: string }).title
        : "API Error"
    );
    this.status = status;
    this.body = body;
  }
}

export async function apiGet<T>(path: string): Promise<T> {
  return apiFetch<T>(path, { method: "GET" });
}

export async function apiGetBlob(path: string): Promise<Blob> {
  const token = getAccessToken();
  const headers: Record<string, string> = {};

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  let res = await fetch(`${API_BASE}${path}`, { method: "GET", headers });

  if (res.status === 401 && token) {
    const refreshed = await refreshAccessToken();
    if (refreshed) {
      headers["Authorization"] = `Bearer ${getAccessToken()}`;
      res = await fetch(`${API_BASE}${path}`, { method: "GET", headers });
    } else {
      clearTokens();
      if (typeof window !== "undefined") {
        window.location.href = "/login";
      }
      throw new Error("Session expired");
    }
  }

  if (!res.ok) {
    const error = await res.json().catch(() => ({ title: "Request failed" }));
    throw new ApiError(res.status, error);
  }

  return res.blob();
}

export async function apiPost<T>(path: string, body: unknown): Promise<T> {
  return apiFetch<T>(path, {
    method: "POST",
    body: JSON.stringify(body),
  });
}

export async function apiPostFormData<T>(path: string, formData: FormData): Promise<T> {
  const token = getAccessToken();
  // Do NOT set Content-Type — the browser sets it with the correct multipart boundary
  const headers: Record<string, string> = {};
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  let res = await fetch(`${API_BASE}${path}`, { method: "POST", headers, body: formData });

  if (res.status === 401 && token) {
    const refreshed = await refreshAccessToken();
    if (refreshed) {
      headers["Authorization"] = `Bearer ${getAccessToken()}`;
      res = await fetch(`${API_BASE}${path}`, { method: "POST", headers, body: formData });
    } else {
      clearTokens();
      if (typeof window !== "undefined") {
        window.location.href = "/login";
      }
      throw new Error("Session expired");
    }
  }

  if (!res.ok) {
    const error = await res.json().catch(() => ({ title: "Request failed" }));
    const errorCode = typeof error === "object" && error !== null && "code" in error
      ? String((error as { code: unknown }).code)
      : undefined;
    trackApiError(path, res.status, errorCode);
    throw new ApiError(res.status, error);
  }

  if (res.status === 204) {
    return undefined as unknown as T;
  }

  return res.json();
}

export async function apiPatch<T>(path: string, body: unknown): Promise<T> {
  return apiFetch<T>(path, {
    method: "PATCH",
    body: JSON.stringify(body),
  });
}

export async function apiPut<T>(path: string, body: unknown): Promise<T> {
  return apiFetch<T>(path, {
    method: "PUT",
    body: JSON.stringify(body),
  });
}

export async function apiDelete(path: string): Promise<void> {
  await apiFetch<void>(path, { method: "DELETE" });
}
