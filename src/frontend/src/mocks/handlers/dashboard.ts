import { http, HttpResponse, delay } from 'msw'
import type {
  DashboardStatsDto,
  TodayAppointmentDto,
  ActivityDto,
} from '@/lib/api/dashboard'

const MOCK_STATS: DashboardStatsDto = {
  appointmentsToday: 8,
  pendingCheckin: 3,
  unpaidInvoicesAed: 2450.0,
  totalPatients: 127,
}

// Today's date in ISO for mock appointments
const today = new Date()
const todayStr = today.toISOString().split('T')[0]

const MOCK_TODAY_APPOINTMENTS: TodayAppointmentDto[] = [
  {
    id: 'appt-0000-0000-0000-000000000001',
    patientName: 'Max',
    species: 'Dog',
    ownerName: 'Ahmed Al-Rashid',
    vetName: 'Dr. Sarah Johnson',
    vetId: 'vet-0000-0000-0000-000000000001',
    status: 'CHECKED_IN',
    scheduledAt: `${todayStr}T09:00:00+04:00`,
  },
  {
    id: 'appt-0000-0000-0000-000000000002',
    patientName: 'Luna',
    species: 'Cat',
    ownerName: 'Fatima Hassan',
    vetName: 'Dr. Ahmed Khalil',
    vetId: 'vet-0000-0000-0000-000000000002',
    status: 'SCHEDULED',
    scheduledAt: `${todayStr}T10:30:00+04:00`,
  },
  {
    id: 'appt-0000-0000-0000-000000000003',
    patientName: 'Rocky',
    species: 'Dog',
    ownerName: 'Mohammed Al-Zaabi',
    vetName: 'Dr. Sarah Johnson',
    vetId: 'vet-0000-0000-0000-000000000001',
    status: 'SCHEDULED',
    scheduledAt: `${todayStr}T11:00:00+04:00`,
  },
  {
    id: 'appt-0000-0000-0000-000000000004',
    patientName: 'Bella',
    species: 'Cat',
    ownerName: 'Noura Al-Mansoori',
    vetName: 'Dr. Ahmed Khalil',
    vetId: 'vet-0000-0000-0000-000000000002',
    status: 'SCHEDULED',
    scheduledAt: `${todayStr}T12:00:00+04:00`,
  },
  {
    id: 'appt-0000-0000-0000-000000000005',
    patientName: 'Charlie',
    species: 'Bird',
    ownerName: 'Khalid Al-Nuaimi',
    vetName: 'Dr. Sarah Johnson',
    vetId: 'vet-0000-0000-0000-000000000001',
    status: 'IN_PROGRESS',
    scheduledAt: `${todayStr}T13:30:00+04:00`,
  },
]

const MOCK_RECENT_ACTIVITY: ActivityDto[] = [
  {
    id: 'act-001',
    type: 'BILLING',
    message: 'Invoice #INV-2026-003 paid -- AED 2,310.00',
    occurredAt: new Date(Date.now() - 30 * 60_000).toISOString(),
    relatedId: 'inv-0000-0000-0000-000000000003',
  },
  {
    id: 'act-002',
    type: 'MEDICAL',
    message: 'Medical record created -- Luna (Dr. Ahmed Khalil)',
    occurredAt: new Date(Date.now() - 105 * 60_000).toISOString(),
    relatedId: 'pat-0000-0000-0000-000000000002',
  },
  {
    id: 'act-003',
    type: 'APPOINTMENT',
    message: 'Appointment completed -- Max (Dr. Sarah Johnson)',
    occurredAt: new Date(Date.now() - 180 * 60_000).toISOString(),
    relatedId: 'appt-0000-0000-0000-000000000001',
  },
  {
    id: 'act-004',
    type: 'BILLING',
    message: 'Invoice #INV-2026-002 sent -- AED 241.50',
    occurredAt: new Date(Date.now() - 240 * 60_000).toISOString(),
    relatedId: 'inv-0000-0000-0000-000000000002',
  },
  {
    id: 'act-005',
    type: 'APPOINTMENT',
    message: 'Appointment created -- Rocky, 11:00 (Dr. Sarah Johnson)',
    occurredAt: new Date(Date.now() - 300 * 60_000).toISOString(),
    relatedId: 'appt-0000-0000-0000-000000000003',
  },
  {
    id: 'act-006',
    type: 'MEDICAL',
    message: 'Prescription added -- Bella (Dr. Ahmed Khalil)',
    occurredAt: new Date(Date.now() - 360 * 60_000).toISOString(),
    relatedId: 'pat-0000-0000-0000-000000000004',
  },
  {
    id: 'act-007',
    type: 'APPOINTMENT',
    message: 'Appointment cancelled -- Milo (Dr. Sarah Johnson)',
    occurredAt: new Date(Date.now() - 420 * 60_000).toISOString(),
    relatedId: null,
  },
  {
    id: 'act-008',
    type: 'BILLING',
    message: 'Invoice #INV-2026-001 created -- AED 210.00',
    occurredAt: new Date(Date.now() - 480 * 60_000).toISOString(),
    relatedId: 'inv-0000-0000-0000-000000000001',
  },
  {
    id: 'act-009',
    type: 'MEDICAL',
    message: 'Examination added -- Charlie (Dr. Sarah Johnson)',
    occurredAt: new Date(Date.now() - 540 * 60_000).toISOString(),
    relatedId: null,
  },
  {
    id: 'act-010',
    type: 'APPOINTMENT',
    message: 'Appointment created -- Bella, 12:00 (Dr. Ahmed Khalil)',
    occurredAt: new Date(Date.now() - 600 * 60_000).toISOString(),
    relatedId: 'appt-0000-0000-0000-000000000004',
  },
]

export const dashboardHandlers = [
  // GET /api/dashboard/stats
  http.get('/api/dashboard/stats', async () => {
    await delay(150)
    return HttpResponse.json<DashboardStatsDto>(MOCK_STATS)
  }),

  // GET /api/dashboard/today-appointments
  http.get('/api/dashboard/today-appointments', async () => {
    await delay(150)
    return HttpResponse.json<TodayAppointmentDto[]>(MOCK_TODAY_APPOINTMENTS)
  }),

  // GET /api/dashboard/recent-activity
  http.get('/api/dashboard/recent-activity', async () => {
    await delay(150)
    return HttpResponse.json<ActivityDto[]>(MOCK_RECENT_ACTIVITY)
  }),
]
