import { apiGet } from './client'
import { getPortalToken } from './portal'
import { ApiError } from './client'

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? ''
const PORTAL_BOOKING_BASE = '/api/v1/portal/booking'

// ─── Portal booking fetch (uses MagicLink auth) ───────────────────────────────

async function bookingFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
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

// ─── Types ────────────────────────────────────────────────────────────────────

export interface BookingSlot {
  /** ISO 8601 datetime in Asia/Dubai timezone offset */
  startsAt: string
  endsAt: string
  vetId: string
  vetName: string
  available: boolean
}

export interface BookingDay {
  date: string // YYYY-MM-DD in Asia/Dubai
  slots: BookingSlot[]
  /** true if the clinic is closed this day */
  closed: boolean
}

export interface SlotSuggestionDto {
  vetId: string
  vetName: string
  startsAt: string
  endsAt: string
  score: number
}

// ─── Portal booking types ─────────────────────────────────────────────────────

export interface ConsultationTypeDto {
  id: string
  name: string
  durationMinutes: number
  description: string
}

export interface VeterinarianDto {
  id: string
  name: string
  specialties: string[]
}

export interface BookingPetDto {
  id: string
  name: string
  species: string
  breed: string
  ageYears: number
}

export interface CreateBookingAppointmentRequest {
  petId: string
  consultationTypeId: string
  vetId: string | null
  slotStartsAt: string
  slotEndsAt: string
  reason?: string
}

export interface BookingAppointmentDto {
  id: string
  petId: string
  consultationTypeId: string
  vetId: string | null
  slotStartsAt: string
  slotEndsAt: string
  reason?: string
  createdAt: string
}

export interface SuggestSlotsParams {
  /** YYYY-MM-DD — search window start */
  from: string
  /** YYYY-MM-DD — search window end */
  to: string
  /** optional vet preference */
  vetId?: string
  /** reason / chief complaint for context */
  reason?: string
  /** duration in minutes (default 30) */
  durationMinutes?: number
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function getWeekSlots(params: {
  weekStart: string // YYYY-MM-DD (Sunday)
  vetId?: string
  durationMinutes?: number
}): Promise<BookingDay[]> {
  const searchParams = new URLSearchParams({ weekStart: params.weekStart })
  if (params.vetId) searchParams.set('vetId', params.vetId)
  if (params.durationMinutes) searchParams.set('durationMinutes', String(params.durationMinutes))
  return apiGet<BookingDay[]>(`/api/v1/booking/slots?${searchParams.toString()}`)
}

export async function suggestSlots(
  params: SuggestSlotsParams
): Promise<SlotSuggestionDto[]> {
  const searchParams = new URLSearchParams({ from: params.from, to: params.to })
  if (params.vetId) searchParams.set('vetId', params.vetId)
  if (params.reason) searchParams.set('reason', params.reason)
  if (params.durationMinutes) searchParams.set('durationMinutes', String(params.durationMinutes))
  return apiGet<SlotSuggestionDto[]>(`/api/v1/booking/suggest?${searchParams.toString()}`)
}

// ─── Portal booking API functions ─────────────────────────────────────────────

export function listPortalConsultationTypes(): Promise<ConsultationTypeDto[]> {
  return bookingFetch<ConsultationTypeDto[]>(`${PORTAL_BOOKING_BASE}/consultation-types`)
}

export function listPortalVeterinarians(): Promise<VeterinarianDto[]> {
  return bookingFetch<VeterinarianDto[]>(`${PORTAL_BOOKING_BASE}/veterinarians`)
}

export function listPortalPets(): Promise<BookingPetDto[]> {
  return bookingFetch<BookingPetDto[]>(`${PORTAL_BOOKING_BASE}/pets`)
}

export function createBookingAppointment(
  request: CreateBookingAppointmentRequest
): Promise<BookingAppointmentDto> {
  return bookingFetch<BookingAppointmentDto>(`${PORTAL_BOOKING_BASE}/appointments`, {
    method: 'POST',
    body: JSON.stringify(request),
  })
}
