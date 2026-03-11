/**
 * Realistic mock data for Booking module.
 * UAE context: Arabic/English names, AED currency, Asia/Dubai timezone.
 * UAE work week: Sunday-Thursday.
 */
import type {
  ConsultationTypeDto,
  VeterinarianDto,
  BookingAppointmentDto,
} from '@/lib/api/booking-types'

// ─── Date helpers ─────────────────────────────────────────────────────────────

/** Returns the next occurrence of a specific day-of-week (0=Sun, 1=Mon, ..., 6=Sat) */
function nextWeekday(dayOfWeek: number): Date {
  const now = new Date()
  const result = new Date(now)
  const currentDay = now.getDay()
  const daysUntil = (dayOfWeek - currentDay + 7) % 7 || 7
  result.setDate(now.getDate() + daysUntil)
  result.setHours(0, 0, 0, 0)
  return result
}

/** Returns a date N days in the past */
function daysAgo(n: number): Date {
  const d = new Date()
  d.setDate(d.getDate() - n)
  return d
}

/** Format a date + time as ISO string in Asia/Dubai timezone offset (+04:00) */
function toUAEIso(date: Date, hour: number, minute = 0): string {
  const d = new Date(date)
  // Set to UTC then offset by +4 to get UAE time
  d.setUTCHours(hour - 4, minute, 0, 0)
  return d.toISOString()
}

// Upcoming Sunday (first UAE workday)
const nextSunday = nextWeekday(0)
// Past Thursday
const pastThursday = daysAgo(8) // ~last week Thursday

// ─── Consultation Types ────────────────────────────────────────────────────────

export const MOCK_CONSULTATION_TYPES: ConsultationTypeDto[] = [
  {
    id: 'ctype-0000-0000-0000-000000000001',
    name: 'General Consultation',
    durationMinutes: 30,
    description: 'General health check and examination',
  },
  {
    id: 'ctype-0000-0000-0000-000000000002',
    name: 'Vaccination',
    durationMinutes: 20,
    description: 'Routine vaccinations and preventive care',
  },
  {
    id: 'ctype-0000-0000-0000-000000000003',
    name: 'Surgery',
    durationMinutes: 45,
    description: 'Minor surgical procedures and interventions',
  },
  {
    id: 'ctype-0000-0000-0000-000000000004',
    name: 'Follow-up',
    durationMinutes: 20,
    description: 'Post-treatment follow-up and monitoring',
  },
  {
    id: 'ctype-0000-0000-0000-000000000005',
    name: 'Emergency',
    durationMinutes: 30,
    description: 'Urgent medical care for acute conditions',
  },
  {
    id: 'ctype-0000-0000-0000-000000000006',
    name: 'Dental',
    durationMinutes: 40,
    description: 'Dental cleaning, extraction, and oral health',
  },
  {
    id: 'ctype-0000-0000-0000-000000000007',
    name: 'Grooming',
    durationMinutes: 60,
    description: 'Full grooming service: bath, haircut, nails',
  },
]

// ─── Veterinarians ────────────────────────────────────────────────────────────

export const MOCK_VETERINARIANS: VeterinarianDto[] = [
  {
    id: 'vet-0000-0000-0000-000000000001',
    name: 'Dr. Ahmed Al-Rashidi',
    specialization: 'Small Animals & Surgery',
    avatarUrl: null,
  },
  {
    id: 'vet-0000-0000-0000-000000000002',
    name: 'Dr. Fatima Al-Zaabi',
    specialization: 'Internal Medicine & Dermatology',
    avatarUrl: null,
  },
  {
    id: 'vet-0000-0000-0000-000000000003',
    name: 'Dr. Sarah Mitchell',
    specialization: 'Dentistry & Preventive Care',
    avatarUrl: null,
  },
]

// ─── Appointments ─────────────────────────────────────────────────────────────

export const MOCK_BOOKING_APPOINTMENTS: BookingAppointmentDto[] = [
  {
    id: 'appt-0000-0000-0000-000000000001',
    consultationTypeId: 'ctype-0000-0000-0000-000000000001',
    consultationTypeName: 'General Consultation',
    veterinarianId: 'vet-0000-0000-0000-000000000001',
    veterinarianName: 'Dr. Ahmed Al-Rashidi',
    startTime: toUAEIso(nextSunday, 10, 0),
    endTime: toUAEIso(nextSunday, 10, 30),
    petName: 'Max',
    petSpecies: 'Dog',
    ownerName: 'Khalid Al-Nuaimi',
    ownerPhone: '+971501234567',
    ownerEmail: 'khalid.alnuaimi@email.ae',
    notes: 'Regular annual check-up',
    status: 'Scheduled',
    rescheduleCount: 0,
    createdAt: daysAgo(3).toISOString(),
  },
  {
    id: 'appt-0000-0000-0000-000000000002',
    consultationTypeId: 'ctype-0000-0000-0000-000000000004',
    consultationTypeName: 'Follow-up',
    veterinarianId: 'vet-0000-0000-0000-000000000002',
    veterinarianName: 'Dr. Fatima Al-Zaabi',
    startTime: toUAEIso(pastThursday, 14, 0),
    endTime: toUAEIso(pastThursday, 14, 20),
    petName: 'Luna',
    petSpecies: 'Cat',
    ownerName: 'Mariam Hassan',
    ownerPhone: '+971509876543',
    ownerEmail: 'mariam.hassan@email.ae',
    notes: 'Post-surgery follow-up after splenectomy',
    status: 'Completed',
    rescheduleCount: 1,
    createdAt: daysAgo(14).toISOString(),
  },
]
