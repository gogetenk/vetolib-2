/**
 * Realistic mock data for the Booking portal module.
 * UAE context: Arabic/English names, Asia/Dubai timezone.
 * UAE work week: Sunday-Thursday.
 */
import type { BookingAppointmentDto, ConsultationTypeDto, VeterinarianDto } from '@/lib/api/booking-types'

// ─── Consultation Types ───────────────────────────────────────────────────────

export const MOCK_CONSULTATION_TYPES: ConsultationTypeDto[] = [
  {
    id: 'ct-00000000-0000-0000-0000-000000000001',
    name: 'General Consultation',
    durationMinutes: 30,
    description: 'Routine check-up and general health examination',
    price: 250,
    isActive: true,
    sortOrder: 1,
    colorHex: '#4CAF50',
  },
  {
    id: 'ct-00000000-0000-0000-0000-000000000002',
    name: 'Vaccination',
    durationMinutes: 20,
    description: 'Annual vaccinations and booster shots',
    price: 150,
    isActive: true,
    sortOrder: 2,
    colorHex: '#2196F3',
  },
  {
    id: 'ct-00000000-0000-0000-0000-000000000003',
    name: 'Dental Cleaning',
    durationMinutes: 60,
    description: 'Professional dental scaling and polishing under anesthesia',
    price: 800,
    isActive: true,
    sortOrder: 3,
    colorHex: '#FF9800',
  },
  {
    id: 'ct-00000000-0000-0000-0000-000000000004',
    name: 'Emergency',
    durationMinutes: 45,
    description: 'Urgent care for critical conditions',
    price: 500,
    isActive: true,
    sortOrder: 4,
    colorHex: '#F44336',
  },
  {
    id: 'ct-00000000-0000-0000-0000-000000000005',
    name: 'Surgery Consultation',
    durationMinutes: 45,
    description: 'Pre-operative assessment and surgical planning',
    price: 350,
    isActive: true,
    sortOrder: 5,
    colorHex: '#9C27B0',
  },
]

// ─── Veterinarians ────────────────────────────────────────────────────────────

export const MOCK_VETERINARIANS: VeterinarianDto[] = [
  {
    id: 'vet-00000000-0000-0000-0000-000000000001',
    name: 'Dr. Ahmed Al-Rashidi',
    speciality: 'Small Animals',
    specializations: ['Small Animals', 'Internal Medicine'],
    avatarUrl: null,
  },
  {
    id: 'vet-00000000-0000-0000-0000-000000000002',
    name: 'Dr. Fatima Al-Zaabi',
    speciality: 'Surgery',
    specializations: ['Surgery', 'Orthopedics'],
    avatarUrl: null,
  },
  {
    id: 'vet-00000000-0000-0000-0000-000000000003',
    name: 'Dr. Sarah Mitchell',
    speciality: 'Dentistry',
    specializations: ['Dentistry', 'Small Animals'],
    avatarUrl: null,
  },
]

// ─── Date helpers ─────────────────────────────────────────────────────────────

const UAE_WORK_DAYS = new Set([0, 1, 2, 3, 4]) // Sun=0 … Thu=4

export function getNextWorkingDay(dayOffset = 1): Date {
  const d = new Date()
  d.setHours(8, 0, 0, 0)
  for (let i = 0; i < dayOffset; i++) d.setDate(d.getDate() + 1)
  while (!UAE_WORK_DAYS.has(d.getDay())) d.setDate(d.getDate() + 1)
  return d
}

export function toISOAt(d: Date, hour: number, minute = 0): string {
  const year = d.getFullYear()
  const month = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  const h = String(hour).padStart(2, '0')
  const m = String(minute).padStart(2, '0')
  return `${year}-${month}-${day}T${h}:${m}:00+04:00`
}

// ─── Mock appointments ────────────────────────────────────────────────────────

export function getMockOwnerAppointments(): BookingAppointmentDto[] {
  const futureDay = getNextWorkingDay(2)
  const pastDay = new Date(Date.now() - 7 * 24 * 60 * 60 * 1000)

  return [
    {
      id: 'bk-appt-0000-0000-0000-000000000001',
      consultationTypeId: 'ct-00000000-0000-0000-0000-000000000002',
      consultationTypeName: 'Vaccination',
      veterinarianId: 'vet-00000000-0000-0000-0000-000000000003',
      veterinarianName: 'Dr. Sarah Mitchell',
      petName: 'Max',
      scheduledAt: toISOAt(futureDay, 10, 0),
      durationMinutes: 20,
      status: 'Scheduled',
      notes: 'Annual rabies and DHPP booster.',
      clinicName: 'Desert Paws Veterinary Clinic',
      clinicAddress: 'Al Wasl Road, Jumeirah 1, Dubai, UAE',
    },
    {
      id: 'bk-appt-0000-0000-0000-000000000002',
      consultationTypeId: 'ct-00000000-0000-0000-0000-000000000001',
      consultationTypeName: 'General Consultation',
      veterinarianId: 'vet-00000000-0000-0000-0000-000000000001',
      veterinarianName: 'Dr. Ahmed Al-Rashidi',
      petName: 'Luna',
      scheduledAt: toISOAt(pastDay, 14, 30),
      durationMinutes: 30,
      status: 'Completed',
      notes: null,
      clinicName: 'Desert Paws Veterinary Clinic',
      clinicAddress: 'Al Wasl Road, Jumeirah 1, Dubai, UAE',
    },
    {
      id: 'bk-appt-0000-0000-0000-000000000003',
      consultationTypeId: 'ct-00000000-0000-0000-0000-000000000003',
      consultationTypeName: 'Surgery',
      veterinarianId: 'vet-00000000-0000-0000-0000-000000000002',
      veterinarianName: 'Dr. Fatima Al-Zaabi',
      petName: 'Rocky',
      scheduledAt: toISOAt(new Date(Date.now() - 14 * 24 * 60 * 60 * 1000), 9, 0),
      durationMinutes: 45,
      status: 'Cancelled',
      notes: 'Owner requested cancellation.',
      clinicName: 'Desert Paws Veterinary Clinic',
      clinicAddress: 'Al Wasl Road, Jumeirah 1, Dubai, UAE',
    },
  ]
}
