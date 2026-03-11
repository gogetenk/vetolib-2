/**
 * Booking module TypeScript types.
 * These match the Vetolib.Agenda.Contracts DTOs for the owner portal booking flow.
 */

// ─── Consultation Types ───────────────────────────────────────────────────────

export interface ConsultationTypeDto {
  id: string
  name: string
  durationMinutes: number
  priceAed: number
  description: string
  isActive: boolean
  sortOrder: number
  requiresVetSelection: boolean
  colorHex: string
}

// ─── Veterinarians ────────────────────────────────────────────────────────────

export interface VeterinarianDto {
  id: string
  fullName: string
  vetLicenseNumber: string
  specializations: string[]
  avatarUrl: string | null
}

// ─── Availability ─────────────────────────────────────────────────────────────

export interface TimeSlotDto {
  startTime: string  // "HH:mm"
  endTime: string    // "HH:mm"
  isAvailable: boolean
}

export interface DayAvailabilityDto {
  date: string       // "YYYY-MM-DD"
  slots: TimeSlotDto[]
}

// ─── Booking Appointments ─────────────────────────────────────────────────────

export type BookingAppointmentStatus =
  | 'Scheduled'
  | 'CheckedIn'
  | 'Completed'
  | 'Cancelled'
  | 'NoShow'

export interface BookingAppointmentDto {
  id: string
  date: string           // "YYYY-MM-DD"
  startTime: string      // "HH:mm"
  endTime: string        // "HH:mm"
  animalName: string
  consultationTypeName: string
  veterinarianName: string
  clinicName: string
  clinicAddress: string
  status: BookingAppointmentStatus
  notes: string | null
}

// ─── Request types ────────────────────────────────────────────────────────────

export interface CreateBookingRequest {
  consultationTypeId: string
  veterinarianId: string | null
  date: string           // "YYYY-MM-DD"
  startTime: string      // "HH:mm"
  animalId: string
  animalName: string
  reason: string
}

export interface GetAvailabilityRequest {
  date: string           // "YYYY-MM-DD"
  veterinarianId?: string | null
  consultationTypeId: string
}
