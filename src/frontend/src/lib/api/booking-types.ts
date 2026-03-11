/**
 * Types for the Booking (Portal) module.
 * UAE context: AED currency, Asia/Dubai timezone, Sunday-Thursday work week.
 */

export interface ConsultationTypeDto {
  id: string
  name: string
  durationMinutes: number
  description: string
}

export interface VeterinarianDto {
  id: string
  name: string
  specialization: string
  avatarUrl: string | null
}

export interface TimeSlotDto {
  startTime: string // ISO 8601
  endTime: string   // ISO 8601
  veterinarianId: string
  veterinarianName: string
  isAvailable: boolean
}

export interface AvailabilityResponseDto {
  date: string // YYYY-MM-DD
  slots: TimeSlotDto[]
}

export interface SlotSuggestionDto {
  startTime: string
  endTime: string
  veterinarianId: string
  veterinarianName: string
  score: number
}

export interface SuggestSlotRequest {
  consultationTypeId: string
  preferredDates: string[] // YYYY-MM-DD
  veterinarianId?: string
}

export interface CreateBookingRequest {
  consultationTypeId: string
  veterinarianId: string
  startTime: string // ISO 8601
  petName: string
  petSpecies: string
  ownerName: string
  ownerPhone: string
  ownerEmail: string
  notes?: string
}

export interface BookingAppointmentDto {
  id: string
  consultationTypeId: string
  consultationTypeName: string
  veterinarianId: string
  veterinarianName: string
  startTime: string
  endTime: string
  petName: string
  petSpecies: string
  ownerName: string
  ownerPhone: string
  ownerEmail: string
  notes: string | null
  status: 'Scheduled' | 'Completed' | 'Cancelled' | 'NoShow'
  rescheduleCount: number
  createdAt: string
}

export interface CancelBookingRequest {
  reason?: string
}

export interface RescheduleBookingRequest {
  newStartTime: string // ISO 8601
  reason?: string
}
