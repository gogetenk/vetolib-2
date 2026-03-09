/**
 * Auth helpers for client-side session management.
 * Tokens are stored in localStorage (access_token, refresh_token).
 */

export interface Session {
  accessToken: string;
  refreshToken: string | null;
}

export function getSession(): Session | null {
  if (typeof window === "undefined") return null;

  const accessToken = localStorage.getItem("access_token");
  if (!accessToken) return null;

  return {
    accessToken,
    refreshToken: localStorage.getItem("refresh_token"),
  };
}

export function isAuthenticated(): boolean {
  return getSession() !== null;
}

export function setSession(accessToken: string, refreshToken: string): void {
  localStorage.setItem("access_token", accessToken);
  localStorage.setItem("refresh_token", refreshToken);
}

export function clearSession(): void {
  localStorage.removeItem("access_token");
  localStorage.removeItem("refresh_token");
}

export function redirectToLogin(): void {
  if (typeof window !== "undefined") {
    window.location.href = "/login";
  }
}
