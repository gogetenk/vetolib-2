import { ApiError, apiPost } from './client'
import { posthog, isPostHogAvailable } from '@/lib/posthog'
import { parseJwt } from '@/lib/jwt'

export interface AuthTokens {
  accessToken: string
  refreshToken: string
  expiresIn: number
}

export interface UserInfo {
  email: string
  name: string
  clinicId: string
  clinicName: string
}

export interface LoginResponse extends AuthTokens {
  user: UserInfo
}

export type LoginErrorCode = 'INVALID_CREDENTIALS' | 'ACCOUNT_LOCKED' | 'NETWORK_ERROR'

export interface LoginError {
  code: LoginErrorCode
  message: string
}

// Decision: tokens stored in localStorage for simplicity in MVP.
// httpOnly cookie via Next.js route would require a server route.
// Documented here for wire task.

export function storeTokens(tokens: AuthTokens): void {
  if (typeof window === 'undefined') return
  localStorage.setItem('access_token', tokens.accessToken)
  localStorage.setItem('refresh_token', tokens.refreshToken)
  // Also set a cookie so Next.js middleware can detect authentication on server
  document.cookie = `access_token=${tokens.accessToken}; path=/; SameSite=Lax`
}

export function getStoredAccessToken(): string | null {
  if (typeof window === 'undefined') return null
  return localStorage.getItem('access_token')
}

export function clearStoredTokens(): void {
  if (typeof window === 'undefined') return
  localStorage.removeItem('access_token')
  localStorage.removeItem('refresh_token')
  // Clear the middleware cookie as well
  document.cookie = 'access_token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT'
}

export function getStoredUser(): UserInfo | null {
  if (typeof window === 'undefined') return null
  const token = localStorage.getItem('access_token')
  if (!token) return null
  try {
    const parts = token.split('.')
    const payload = JSON.parse(atob(parts[1]))
    if (payload.clinicId && payload.clinicName && payload.name && payload.sub) {
      return {
        email: payload.sub as string,
        name: payload.name as string,
        clinicId: payload.clinicId as string,
        clinicName: payload.clinicName as string,
      }
    }
    return null
  } catch {
    return null
  }
}

export function isAuthenticated(): boolean {
  return getStoredAccessToken() !== null
}

function identifyUserInPostHog(tokens: AuthTokens, user: UserInfo): void {
  if (!isPostHogAvailable()) return
  const payload = parseJwt(tokens.accessToken)
  // Use the JWT subject as opaque user identifier — must NOT be email/PII.
  // In this backend the sub claim is the user GUID (UUID).
  if (!payload?.sub) return
  posthog.identify(payload.sub, {
    role: payload.role ?? 'unknown',
    clinic_id: user.clinicId,
  })
  posthog.group('clinic', user.clinicId)
}

// POST /api/auth/login
export async function login(email: string, password: string): Promise<LoginResponse> {
  try {
    const response = await apiPost<LoginResponse>('/api/auth/login', { email, password })
    storeTokens(response)
    identifyUserInPostHog(response, response.user)
    return response
  } catch (err) {
    if (err instanceof ApiError) {
      const body = err.body as { code?: string; title?: string }
      if (body?.code === 'ACCOUNT_LOCKED') {
        throw { code: 'ACCOUNT_LOCKED', message: 'Account locked. Try again in 15 minutes.' } as LoginError
      }
      throw { code: 'INVALID_CREDENTIALS', message: 'Invalid email or password' } as LoginError
    }
    throw { code: 'NETWORK_ERROR', message: 'Connection error. Please try again.' } as LoginError
  }
}

// POST /api/auth/refresh
export async function refreshTokens(refreshToken: string): Promise<AuthTokens> {
  const response = await apiPost<AuthTokens>('/api/auth/refresh', { refreshToken })
  storeTokens(response)
  return response
}

export function logout(): void {
  clearStoredTokens()
  if (isPostHogAvailable()) {
    posthog.reset()
  }
}

export type RegisterErrorCode = 'EMAIL_TAKEN' | 'NETWORK_ERROR'

export interface RegisterError {
  code: RegisterErrorCode
  message: string
}

export interface RegisterClinicRequest {
  clinicName: string
  email: string
  password: string
  phone: string
}

// POST /api/auth/register-clinic
export async function registerClinic(data: RegisterClinicRequest): Promise<void> {
  try {
    await apiPost<void>('/api/auth/register-clinic', data)
  } catch (err) {
    if (err instanceof ApiError && err.status === 409) {
      throw { code: 'EMAIL_TAKEN', message: 'Email already in use' } as RegisterError
    }
    throw { code: 'NETWORK_ERROR', message: 'Connection error. Please try again.' } as RegisterError
  }
}
