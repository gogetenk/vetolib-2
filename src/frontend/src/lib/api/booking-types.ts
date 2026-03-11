// ─── Booking Portal Types ──────────────────────────────────────────────────────
// Public booking portal — UAE market (AED, Asia/Dubai, Sun-Thu work week)

export interface ConsultationTypeDto {
  id: string
  name: string
  durationMinutes: number
  description: string | null
  price: number // AED
  isActive: boolean
  sortOrder: number
  colorHex: string
}

export interface VeterinarianDto {
  id: string
  name: string
  speciality: string | null
  specializations: string[]
  avatarUrl: string | null
}

export interface TimeSlotDto {
  time: string // "HH:mm"
  isAvailable: boolean
}

export interface AvailabilityDayDto {
  date: string // ISO date "YYYY-MM-DD"
  slots: TimeSlotDto[]
}

export type BookingAppointmentStatus = 'Scheduled' | 'Completed' | 'Cancelled' | 'NoShow'

export interface BookingAppointmentDto {
  id: string
  consultationTypeId: string
  consultationTypeName: string
  veterinarianId: string
  veterinarianName: string
  petName: string
  scheduledAt: string // ISO datetime
  durationMinutes: number
  status: BookingAppointmentStatus
  notes: string | null
  clinicName: string
  clinicAddress: string
}

export interface CreateBookingAppointmentRequest {
  consultationTypeId: string
  veterinarianId: string
  petName: string
  scheduledAt: string
  notes: string | null
}

export interface CancelBookingAppointmentRequest {
  reason: string | null
}

export interface RescheduleBookingAppointmentRequest {
  newScheduledAt: string
}

export interface SuggestSlotRequest {
  consultationTypeId: string
  veterinarianId: string | null
  preferredDate: string | null
}

export interface SuggestSlotResponse {
  suggestedDate: string
  suggestedTime: string
  veterinarianId: string
  veterinarianName: string
}
