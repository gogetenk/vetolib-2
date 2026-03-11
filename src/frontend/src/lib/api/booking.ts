import { ApiError } from './client'
import type {
  BookingAppointmentDto,
  CreateBookingAppointmentRequest,
  CancelBookingAppointmentRequest,
  RescheduleBookingAppointmentRequest,
  ConsultationTypeDto,
  VeterinarianDto,
  AvailabilityDayDto,
} from './booking-types'

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? ''
const BASE = '/api/v1/portal/booking'

// ─── Token management ─────────────────────────────────────────────────────────

function getPortalToken(): string | null {
  if (typeof window === 'undefined') return null
  return sessionStorage.getItem('portal_token')
}

// ─── Fetch helpers ────────────────────────────────────────────────────────────

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

// ─── Wizard endpoints ─────────────────────────────────────────────────────────

export function getConsultationTypes(clinicSlug: string): Promise<ConsultationTypeDto[]> {
  return bookingFetch<ConsultationTypeDto[]>(`${BASE}/clinics/${clinicSlug}/consultation-types`)
}

export function getVeterinarians(clinicSlug: string): Promise<VeterinarianDto[]> {
  return bookingFetch<VeterinarianDto[]>(`${BASE}/clinics/${clinicSlug}/veterinarians`)
}

export function getAvailability(
  clinicSlug: string,
  params: { date: string; consultationTypeId: string; veterinarianId?: string | null }
): Promise<AvailabilityDayDto> {
  const searchParams = new URLSearchParams({
    date: params.date,
    consultationTypeId: params.consultationTypeId,
  })
  if (params.veterinarianId) {
    searchParams.set('veterinarianId', params.veterinarianId)
  }
  return bookingFetch<AvailabilityDayDto>(
    `${BASE}/clinics/${clinicSlug}/availability?${searchParams.toString()}`
  )
}

// ─── Appointment endpoints ────────────────────────────────────────────────────

export function listBookingAppointments(): Promise<BookingAppointmentDto[]> {
  return bookingFetch<BookingAppointmentDto[]>(`${BASE}/appointments`)
}

export function getBookingAppointment(id: string): Promise<BookingAppointmentDto> {
  return bookingFetch<BookingAppointmentDto>(`${BASE}/appointments/${id}`)
}

export function createBookingAppointment(
  request: CreateBookingAppointmentRequest
): Promise<BookingAppointmentDto> {
  return bookingFetch<BookingAppointmentDto>(`${BASE}/appointments`, {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function cancelBookingAppointment(
  id: string,
  request: CancelBookingAppointmentRequest
): Promise<BookingAppointmentDto> {
  return bookingFetch<BookingAppointmentDto>(`${BASE}/appointments/${id}/cancel`, {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function rescheduleBookingAppointment(
  id: string,
  request: RescheduleBookingAppointmentRequest
): Promise<BookingAppointmentDto> {
  return bookingFetch<BookingAppointmentDto>(`${BASE}/appointments/${id}/reschedule`, {
    method: 'POST',
    body: JSON.stringify(request),
  })
}
