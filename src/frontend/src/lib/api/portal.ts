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

export async function portalFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
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


// ─── Clinic Info ──────────────────────────────────────────────────────────────

export interface PortalClinicInfoDto {
  name: string
  address: string
  phone: string
  openingHours: string
}

export function getPortalClinicInfo(): Promise<PortalClinicInfoDto> {
  return portalFetch<PortalClinicInfoDto>(`${BASE}/clinic-info`)
}

// ─── Pets ──────────────────────────────────────────────────────────────────────

export function listPortalPets(): Promise<PortalPetDto[]> {
  return portalFetch<PortalPetDto[]>(`${BASE}/pets`)
}

// ─── My Animals (new portal endpoints) ───────────────────────────────────────

export interface PortalAnimalDto {
  id: string
  name: string
  species: string
  breed: string
  dateOfBirth: string | null
  lastVisitDate: string | null
}

export interface PortalMedicalRecordDto {
  id: string
  visitDate: string
  reason: string
  diagnosis: string
  treatment: string
  vetName: string
}

export interface PortalVaccinationDto {
  id: string
  name: string
  administeredAt: string
  nextDueAt: string | null
  vetName: string
}

export interface PortalPrescriptionDto {
  id: string
  drugName: string
  dosage: string
  frequency: string
  startDate: string
  endDate: string | null
  prescribedBy: string
}

export interface PortalWeightEntryDto {
  date: string
  weightKg: number
}

export function listMyAnimals(): Promise<PortalAnimalDto[]> {
  return portalFetch<PortalAnimalDto[]>(`${BASE}/my-animals`)
}

export function getAnimalRecords(animalId: string): Promise<PortalMedicalRecordDto[]> {
  return portalFetch<PortalMedicalRecordDto[]>(`${BASE}/animals/${animalId}/records`)
}

export function getAnimalVaccinations(animalId: string): Promise<PortalVaccinationDto[]> {
  return portalFetch<PortalVaccinationDto[]>(`${BASE}/animals/${animalId}/vaccinations`)
}

export function getAnimalPrescriptions(animalId: string): Promise<PortalPrescriptionDto[]> {
  return portalFetch<PortalPrescriptionDto[]>(`${BASE}/animals/${animalId}/prescriptions`)
}

export function getAnimalWeightHistory(animalId: string): Promise<PortalWeightEntryDto[]> {
  return portalFetch<PortalWeightEntryDto[]>(`${BASE}/animals/${animalId}/weight`)
}

// ─── Vaccination Reminders ──────────────────────────────────────────────────

export type VaccinationReminderStatus = 'Upcoming' | 'Overdue' | 'Completed'

export interface VaccinationReminderDto {
  id: string
  vaccineName: string
  dueDate: string
  status: VaccinationReminderStatus
  animalId: string
  animalName: string
}

export function getAnimalVaccinationReminders(
  animalId: string
): Promise<VaccinationReminderDto[]> {
  return portalFetch<VaccinationReminderDto[]>(
    `${BASE}/animals/${animalId}/vaccination-reminders`
  )
}

// ─── Notification Preferences ───────────────────────────────────────────────

export interface NotificationPreferencesDto {
  whatsappEnabled: boolean
  emailEnabled: boolean
}

export function getNotificationPreferences(): Promise<NotificationPreferencesDto> {
  return portalFetch<NotificationPreferencesDto>(`${BASE}/notification-preferences`)
}

export function updateNotificationPreferences(
  body: NotificationPreferencesDto
): Promise<NotificationPreferencesDto> {
  return portalFetch<NotificationPreferencesDto>(`${BASE}/notification-preferences`, {
    method: 'PUT',
    body: JSON.stringify(body),
  })
}
