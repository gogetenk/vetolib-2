import { portalFetch } from './portal'

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? ''
const PORTAL_BASE = '/api/v1/portal'
const PUBLIC_BASE = '/api/v1/shared'

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ShareLinkDto {
  id: string
  animalId: string
  token: string
  shareUrl: string
  expiresAt: string
  createdAt: string
  accessCount: number
  isRevoked: boolean
}

export interface SharedRecordDto {
  animalName: string
  species: string
  breed: string
  ageYears: number
  clinicName: string
  lastVisit: string
  vaccinations: Array<{
    name: string
    date: string
    nextDue: string | null
  }>
  recentConsultations: Array<{
    date: string
    reason: string
    veterinarian: string
    notes: string
  }>
}

// ─── Portal endpoints (require MagicLink auth) ───────────────────────────────

export function createShareLink(animalId: string): Promise<ShareLinkDto> {
  return portalFetch<ShareLinkDto>(`${PORTAL_BASE}/animals/${animalId}/share`, {
    method: 'POST',
  })
}

export function listShareLinks(): Promise<ShareLinkDto[]> {
  return portalFetch<ShareLinkDto[]>(`${PORTAL_BASE}/shares`)
}

export function revokeShareLink(id: string): Promise<void> {
  return portalFetch<void>(`${PORTAL_BASE}/shares/${id}`, {
    method: 'DELETE',
  })
}

// ─── Public endpoint (no auth) ────────────────────────────────────────────────

export async function getSharedRecord(token: string): Promise<SharedRecordDto> {
  const res = await fetch(`${API_BASE}${PUBLIC_BASE}/${token}`, {
    method: 'GET',
    headers: { 'Content-Type': 'application/json' },
  })

  if (!res.ok) {
    const error = await res.json().catch(() => ({ title: 'Request failed' }))
    throw new Error(
      typeof error === 'object' && error !== null && 'title' in error
        ? (error as { title: string }).title
        : 'Failed to load shared record'
    )
  }

  return res.json()
}
