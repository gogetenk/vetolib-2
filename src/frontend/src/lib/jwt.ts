/**
 * Single source of truth for JWT parsing.
 * All JWT decode operations must use parseJwt() from this file.
 */

export interface JwtPayload {
  sub: string
  name: string
  role: string
  clinic_id: string
  clinic_name: string
  exp: number
  iat: number
  // Microsoft identity claim fallback for role
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string
}

/**
 * Decode a JWT token and return its payload.
 * Returns null if the token is invalid or cannot be parsed.
 */
export function parseJwt(token: string): JwtPayload | null {
  try {
    const parts = token.split('.')
    if (parts.length !== 3) return null
    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/')
    const json = atob(base64)
    return JSON.parse(json) as JwtPayload
  } catch {
    return null
  }
}

/**
 * Extract the role from a JWT payload, checking both the direct claim
 * and the Microsoft identity claim fallback.
 */
export function getRoleFromPayload(payload: JwtPayload): string {
  return (
    payload.role ||
    payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
    'VET'
  )
}
