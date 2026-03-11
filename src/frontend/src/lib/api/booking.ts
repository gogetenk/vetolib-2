import { apiGet } from './client'
import { getPortalToken } from './portal'
import { ApiError } from './client'
import type {
  BookingAppointmentDto,
  CreateBookingAppointmentRequest,
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

// ─── Portal Booking (MagicLink auth) ──────────────────────────────────────────

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? ''
const PORTAL_BOOKING_BASE = '/api/v1/portal/booking'

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

export function listBookingAppointments(): Promise<BookingAppointmentDto[]> {
  return bookingFetch<BookingAppointmentDto[]>(`${PORTAL_BOOKING_BASE}/appointments`)
}

export function getBookingAppointment(id: string): Promise<BookingAppointmentDto> {
  return bookingFetch<BookingAppointmentDto>(`${PORTAL_BOOKING_BASE}/appointments/${id}`)
}

export function createBookingAppointment(
  request: CreateBookingAppointmentRequest
): Promise<BookingAppointmentDto> {
  return bookingFetch<BookingAppointmentDto>(`${PORTAL_BOOKING_BASE}/appointments`, {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function cancelBookingAppointment(
  id: string,
  request: CancelBookingAppointmentRequest
): Promise<void> {
  return bookingFetch<void>(`${PORTAL_BOOKING_BASE}/appointments/${id}/cancel`, {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function rescheduleBookingAppointment(
  id: string,
  request: RescheduleBookingAppointmentRequest
): Promise<BookingAppointmentDto> {
  return bookingFetch<BookingAppointmentDto>(`${PORTAL_BOOKING_BASE}/appointments/${id}/reschedule`, {
    method: 'POST',
    body: JSON.stringify(request),
  })
}
