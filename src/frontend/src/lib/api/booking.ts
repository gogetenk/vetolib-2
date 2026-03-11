import type {
  ConsultationTypeDto,
  VeterinarianDto,
  DayAvailabilityDto,
  BookingAppointmentDto,
  CreateBookingRequest,
  GetAvailabilityRequest,
} from './booking-types'
import { getPortalToken } from './portal'
import { ApiError } from './client'

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? ''
const BASE = '/api/v1/portal'

// ─── Portal fetch for booking (uses MagicLink header) ─────────────────────────

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

// ─── Consultation Types ───────────────────────────────────────────────────────

export function getConsultationTypes(clinicSlug: string): Promise<ConsultationTypeDto[]> {
  return bookingFetch<ConsultationTypeDto[]>(`${BASE}/clinics/${clinicSlug}/consultation-types`)
}

// ─── Veterinarians ────────────────────────────────────────────────────────────

export function getVeterinarians(clinicSlug: string): Promise<VeterinarianDto[]> {
  return bookingFetch<VeterinarianDto[]>(`${BASE}/clinics/${clinicSlug}/veterinarians`)
}

// ─── Availability ─────────────────────────────────────────────────────────────

export function getAvailability(
  clinicSlug: string,
  params: GetAvailabilityRequest
): Promise<DayAvailabilityDto> {
  const searchParams = new URLSearchParams({
    date: params.date,
    consultationTypeId: params.consultationTypeId,
  })
  if (params.veterinarianId) {
    searchParams.set('veterinarianId', params.veterinarianId)
  }
  return bookingFetch<DayAvailabilityDto>(
    `${BASE}/clinics/${clinicSlug}/availability?${searchParams.toString()}`
  )
}

// ─── Bookings ─────────────────────────────────────────────────────────────────

export function createBooking(request: CreateBookingRequest): Promise<BookingAppointmentDto> {
  return bookingFetch<BookingAppointmentDto>(`${BASE}/bookings`, {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function listBookingAppointments(): Promise<BookingAppointmentDto[]> {
  return bookingFetch<BookingAppointmentDto[]>(`${BASE}/bookings`)
}
