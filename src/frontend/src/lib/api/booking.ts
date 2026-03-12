import { apiGet } from './client'
import { portalFetch } from './portal'
import type {
  BookingAppointmentDto as OwnerAppointmentDto,
  CancelBookingAppointmentRequest,
  RescheduleBookingAppointmentRequest,
} from './booking-types'

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

const PORTAL_BOOKING_BASE = '/api/v1/portal/booking'

export function listPortalConsultationTypes(): Promise<ConsultationTypeDto[]> {
  return portalFetch<ConsultationTypeDto[]>(`${PORTAL_BOOKING_BASE}/consultation-types`)
}

export function listPortalVeterinarians(): Promise<VeterinarianDto[]> {
  return portalFetch<VeterinarianDto[]>(`${PORTAL_BOOKING_BASE}/veterinarians`)
}

export function listPortalPets(): Promise<BookingPetDto[]> {
  return portalFetch<BookingPetDto[]>(`${PORTAL_BOOKING_BASE}/pets`)
}

export function listBookingAppointments(): Promise<OwnerAppointmentDto[]> {
  return portalFetch<OwnerAppointmentDto[]>(`${PORTAL_BOOKING_BASE}/appointments`)
}

export function getBookingAppointment(id: string): Promise<OwnerAppointmentDto> {
  return portalFetch<OwnerAppointmentDto>(`${PORTAL_BOOKING_BASE}/appointments/${id}`)
}

export function createBookingAppointment(
  request: CreateBookingAppointmentRequest
): Promise<BookingAppointmentDto> {
  return portalFetch<BookingAppointmentDto>(`${PORTAL_BOOKING_BASE}/appointments`, {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function cancelBookingAppointment(
  id: string,
  request: CancelBookingAppointmentRequest
): Promise<void> {
  return portalFetch<void>(`${PORTAL_BOOKING_BASE}/appointments/${id}/cancel`, {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function rescheduleBookingAppointment(
  id: string,
  request: RescheduleBookingAppointmentRequest
): Promise<OwnerAppointmentDto> {
  return portalFetch<OwnerAppointmentDto>(`${PORTAL_BOOKING_BASE}/appointments/${id}/reschedule`, {
    method: 'POST',
    body: JSON.stringify(request),
  })
}
