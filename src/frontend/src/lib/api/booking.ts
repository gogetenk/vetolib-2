/**
 * Booking API client — Portal (public owner booking).
 * Public endpoints: GET availability, consultation-types, veterinarians, suggest-slot
 * Protected endpoints: POST/GET appointments (MagicLink header)
 */
import type {
  ConsultationTypeDto,
  VeterinarianDto,
  AvailabilityResponseDto,
  SlotSuggestionDto,
  SuggestSlotRequest,
  CreateBookingRequest,
  BookingAppointmentDto,
  CancelBookingRequest,
  RescheduleBookingRequest,
} from './booking-types'
import { ApiError } from './client'

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? ''
const BASE = '/api/v1/portal/booking'

// ─── Token management ──────────────────────────────────────────────────────────

export function getBookingToken(): string | null {
  if (typeof window === 'undefined') return null
  return sessionStorage.getItem('booking_token')
}

export function setBookingToken(token: string): void {
  if (typeof window === 'undefined') return
  sessionStorage.setItem('booking_token', token)
}

export function clearBookingToken(): void {
  if (typeof window === 'undefined') return
  sessionStorage.removeItem('booking_token')
}

// ─── Fetch helpers ─────────────────────────────────────────────────────────────

/** Public fetch — no auth header */
async function publicFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  }

  const res = await fetch(`${API_BASE}${path}`, { ...options, headers })

  if (!res.ok) {
    const error = await res.json().catch(() => ({ title: 'Request failed' }))
    throw new ApiError(res.status, error)
  }

  if (res.status === 204) return undefined as T
  return res.json()
}

/** Protected fetch — MagicLink header */
async function bookingFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getBookingToken()
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

// ─── Public endpoints ──────────────────────────────────────────────────────────

export function listConsultationTypes(): Promise<ConsultationTypeDto[]> {
  return publicFetch<ConsultationTypeDto[]>(`${BASE}/consultation-types`)
}

export function listVeterinarians(): Promise<VeterinarianDto[]> {
  return publicFetch<VeterinarianDto[]>(`${BASE}/veterinarians`)
}

export function getAvailability(
  date: string,
  params?: { veterinarianId?: string; consultationTypeId?: string }
): Promise<AvailabilityResponseDto> {
  const query = new URLSearchParams({ date })
  if (params?.veterinarianId) query.set('veterinarianId', params.veterinarianId)
  if (params?.consultationTypeId) query.set('consultationTypeId', params.consultationTypeId)
  return publicFetch<AvailabilityResponseDto>(`${BASE}/availability?${query.toString()}`)
}

export function suggestSlot(body: SuggestSlotRequest): Promise<SlotSuggestionDto[]> {
  return publicFetch<SlotSuggestionDto[]>(`${BASE}/suggest-slot`, {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

// ─── Protected endpoints (MagicLink) ──────────────────────────────────────────

export function createBookingAppointment(body: CreateBookingRequest): Promise<BookingAppointmentDto> {
  return bookingFetch<BookingAppointmentDto>(`${BASE}/appointments`, {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export function listBookingAppointments(ownerEmail?: string): Promise<BookingAppointmentDto[]> {
  const query = ownerEmail ? `?ownerEmail=${encodeURIComponent(ownerEmail)}` : ''
  return bookingFetch<BookingAppointmentDto[]>(`${BASE}/appointments${query}`)
}

export function getBookingAppointment(id: string): Promise<BookingAppointmentDto> {
  return bookingFetch<BookingAppointmentDto>(`${BASE}/appointments/${id}`)
}

export function cancelBookingAppointment(
  id: string,
  body?: CancelBookingRequest
): Promise<BookingAppointmentDto> {
  return bookingFetch<BookingAppointmentDto>(`${BASE}/appointments/${id}/cancel`, {
    method: 'POST',
    body: JSON.stringify(body ?? {}),
  })
}

export function rescheduleBookingAppointment(
  id: string,
  body: RescheduleBookingRequest
): Promise<BookingAppointmentDto> {
  return bookingFetch<BookingAppointmentDto>(`${BASE}/appointments/${id}/reschedule`, {
    method: 'POST',
    body: JSON.stringify(body),
  })
}
