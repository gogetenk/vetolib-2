/**
 * Realistic mock data for portal booking appointments.
 * UAE context: Arabic/English names, AED currency, Asia/Dubai timezone.
 * Clinic: Desert Paws Veterinary Clinic, Al Wasl Road, Dubai
 */
import type { BookingAppointmentDto } from '@/lib/api/booking-types'

const CLINIC_NAME = 'Desert Paws Veterinary Clinic'
const CLINIC_ADDRESS = 'Al Wasl Road, Jumeirah, Dubai, UAE'

export function getMockOwnerAppointments(): BookingAppointmentDto[] {
  const now = new Date('2026-03-11T10:00:00+04:00')
  const inTwoDays = new Date(now.getTime() + 2 * 24 * 60 * 60 * 1000)
  const inFiveDays = new Date(now.getTime() + 5 * 24 * 60 * 60 * 1000)
  const twoDaysAgo = new Date(now.getTime() - 2 * 24 * 60 * 60 * 1000)
  const oneWeekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000)
  const twoWeeksAgo = new Date(now.getTime() - 14 * 24 * 60 * 60 * 1000)
  const threeWeeksAgo = new Date(now.getTime() - 21 * 24 * 60 * 60 * 1000)

  return [
    {
      id: 'appt-0000-0000-0000-000000000001',
      consultationTypeId: 'ct-001',
      consultationTypeName: 'General Checkup',
      veterinarianId: 'vet-0000-0000-0000-000000000001',
      veterinarianName: 'Dr. Sarah Johnson',
      petName: 'Noor',
      scheduledAt: inTwoDays.toISOString(),
      durationMinutes: 30,
      status: 'Scheduled',
      notes: 'Annual wellness exam. Please bring vaccination records.',
      clinicName: CLINIC_NAME,
      clinicAddress: CLINIC_ADDRESS,
    },
    {
      id: 'appt-0000-0000-0000-000000000002',
      consultationTypeId: 'ct-002',
      consultationTypeName: 'Vaccination',
      veterinarianId: 'vet-0000-0000-0000-000000000002',
      veterinarianName: 'Dr. Omar Al-Rashid',
      petName: 'Zaid',
      scheduledAt: inFiveDays.toISOString(),
      durationMinutes: 20,
      status: 'Scheduled',
      notes: null,
      clinicName: CLINIC_NAME,
      clinicAddress: CLINIC_ADDRESS,
    },
    {
      id: 'appt-0000-0000-0000-000000000003',
      consultationTypeId: 'ct-001',
      consultationTypeName: 'General Checkup',
      veterinarianId: 'vet-0000-0000-0000-000000000003',
      veterinarianName: 'Dr. Layla Al-Mansoori',
      petName: 'Noor',
      scheduledAt: twoDaysAgo.toISOString(),
      durationMinutes: 30,
      status: 'CheckedIn',
      notes: 'Limping on right front paw.',
      clinicName: CLINIC_NAME,
      clinicAddress: CLINIC_ADDRESS,
    },
    {
      id: 'appt-0000-0000-0000-000000000004',
      consultationTypeId: 'ct-003',
      consultationTypeName: 'Dental Care',
      veterinarianId: 'vet-0000-0000-0000-000000000001',
      veterinarianName: 'Dr. Sarah Johnson',
      petName: 'Zaid',
      scheduledAt: oneWeekAgo.toISOString(),
      durationMinutes: 45,
      status: 'Completed',
      notes: 'Full dental cleaning completed. Next visit in 6 months.',
      clinicName: CLINIC_NAME,
      clinicAddress: CLINIC_ADDRESS,
    },
    {
      id: 'appt-0000-0000-0000-000000000005',
      consultationTypeId: 'ct-002',
      consultationTypeName: 'Vaccination',
      veterinarianId: 'vet-0000-0000-0000-000000000002',
      veterinarianName: 'Dr. Omar Al-Rashid',
      petName: 'Noor',
      scheduledAt: twoWeeksAgo.toISOString(),
      durationMinutes: 20,
      status: 'Cancelled',
      notes: null,
      clinicName: CLINIC_NAME,
      clinicAddress: CLINIC_ADDRESS,
    },
    {
      id: 'appt-0000-0000-0000-000000000006',
      consultationTypeId: 'ct-001',
      consultationTypeName: 'General Checkup',
      veterinarianId: 'vet-0000-0000-0000-000000000003',
      veterinarianName: 'Dr. Layla Al-Mansoori',
      petName: 'Zaid',
      scheduledAt: threeWeeksAgo.toISOString(),
      durationMinutes: 30,
      status: 'NoShow',
      notes: null,
      clinicName: CLINIC_NAME,
      clinicAddress: CLINIC_ADDRESS,
    },
  ]
}
