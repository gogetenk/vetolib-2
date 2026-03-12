export type BookingAppointmentStatus = 'Scheduled' | 'Completed' | 'Cancelled' | 'NoShow' | 'CheckedIn'

export interface BookingAppointmentDto {
  id: string
  consultationTypeId: string
  consultationTypeName: string
  veterinarianId: string
  veterinarianName: string
  petName: string
  scheduledAt: string // ISO 8601
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
  reason: string
}

export interface RescheduleBookingAppointmentRequest {
  newScheduledAt: string
}
