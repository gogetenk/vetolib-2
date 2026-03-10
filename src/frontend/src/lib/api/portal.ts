import type {
  PortalConversationDto,
  PortalPetDto,
  CreatePortalConversationRequest,
  SendPortalMessageRequest,
  ConsentRequest,
  ConsentResponse,
  PortalMessageDto,
} from './messaging-types'
import { ApiError } from './client'

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? ''
const BASE = '/api/v1/portal'

// ─── Token management ─────────────────────────────────────────────────────────

export function getPortalToken(): string | null {
  if (typeof window === 'undefined') return null
  return sessionStorage.getItem('portal_token')
}

export function setPortalToken(token: string): void {
  if (typeof window === 'undefined') return
  sessionStorage.setItem('portal_token', token)
}

export function clearPortalToken(): void {
  if (typeof window === 'undefined') return
  sessionStorage.removeItem('portal_token')
}

// ─── Portal fetch (uses MagicLink header) ─────────────────────────────────────

async function portalFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getPortalToken()
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  }
  if (token) {
    headers['Authorization'] = `MagicLink ${token}`
  }

  const res = await fetch(`${API_BASE}${path}`, { ...options, headers })

  if (!res.ok) {
    const error = await res.json().catch(() => ({ title: 'Request failed' }))
    throw new ApiError(res.status, error)
  }

  if (res.status === 204) return undefined as T
  return res.json()
}

async function portalFetchBlob(path: string): Promise<Blob> {
  const token = getPortalToken()
  const headers: Record<string, string> = {}
  if (token) {
    headers['Authorization'] = `MagicLink ${token}`
  }

  const res = await fetch(`${API_BASE}${path}`, { method: 'GET', headers })

  if (!res.ok) {
    const error = await res.json().catch(() => ({ title: 'Request failed' }))
    throw new ApiError(res.status, error)
  }

  return res.blob()
}

// ─── Conversations ─────────────────────────────────────────────────────────────

export function listPortalConversations(): Promise<PortalConversationDto[]> {
  return portalFetch<PortalConversationDto[]>(`${BASE}/conversations`)
}

export function getPortalConversation(id: string): Promise<PortalConversationDto> {
  return portalFetch<PortalConversationDto>(`${BASE}/conversations/${id}`)
}

export function createPortalConversation(
  body: CreatePortalConversationRequest
): Promise<PortalConversationDto> {
  return portalFetch<PortalConversationDto>(`${BASE}/conversations`, {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export function sendPortalMessage(
  id: string,
  body: SendPortalMessageRequest
): Promise<PortalMessageDto> {
  return portalFetch<PortalMessageDto>(`${BASE}/conversations/${id}/messages`, {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

// ─── Consent ───────────────────────────────────────────────────────────────────

export function recordConsent(body: ConsentRequest): Promise<ConsentResponse> {
  return portalFetch<ConsentResponse>(`${BASE}/consent`, {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

// ─── Export ────────────────────────────────────────────────────────────────────

export function exportConversations(): Promise<Blob> {
  return portalFetchBlob(`${BASE}/export`)
}

// ─── Pets ──────────────────────────────────────────────────────────────────────

export function listPortalPets(): Promise<PortalPetDto[]> {
  return portalFetch<PortalPetDto[]>(`${BASE}/pets`)
}
