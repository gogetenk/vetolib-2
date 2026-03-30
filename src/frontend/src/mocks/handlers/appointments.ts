import { http, HttpResponse } from 'msw'
import type { AppointmentDto, VetDto, PagedResult, BookingSource } from '@/lib/api/appointments'

const MOCK_VETS: VetDto[] = [
  { id: 'vet-0000-0000-0000-000000000001', name: 'Dr. Sarah Johnson' },
  { id: 'vet-0000-0000-0000-000000000002', name: 'Dr. Omar Al-Rashid' },
  { id: 'vet-0000-0000-0000-000000000003', name: 'Dr. Layla Al-Mansoori' },
  { id: 'vet-0000-0000-0000-000000000004', name: 'Dr. Khalid Ibrahim' },
]

const CLINIC_ID = 'clinic-001'

/** Helper to build a date string and time string from an ISO-like input */
function toDateAndTime(isoDateStr: string): { date: string; startTime: string; endTime: string } {
  const d = new Date(isoDateStr)
  const date = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
  const startTime = `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}:00`
  // Default endTime 30 min later
  const end = new Date(d.getTime() + 30 * 60_000)
  const endTime = `${String(end.getHours()).padStart(2, '0')}:${String(end.getMinutes()).padStart(2, '0')}:00`
  return { date, startTime, endTime }
}

function computeEndTime(startTime: string, durationMinutes: number): string {
  const [h, m] = startTime.split(':').map(Number)
  const totalMinutes = h * 60 + m + durationMinutes
  const endH = Math.floor(totalMinutes / 60)
  const endM = totalMinutes % 60
  return `${String(endH).padStart(2, '0')}:${String(endM).padStart(2, '0')}:00`
}

interface CalendarAppointment extends AppointmentDto {
  consultationType: string
}

function mockAppointment(
  seq: number,
  overrides: Partial<CalendarAppointment> & Pick<CalendarAppointment, 'animalName' | 'ownerName' | 'veterinarianId' | 'veterinarianName' | 'status' | 'date' | 'startTime' | 'reason'>,
): CalendarAppointment {
  const durationMinutes = overrides.durationMinutes ?? 30
  return {
    id: `apt-0000-0000-0000-00000000000${seq}`,
    clinicId: CLINIC_ID,
    animalId: `animal-0000-0000-0000-00000000000${seq}`,
    durationMinutes,
    endTime: computeEndTime(overrides.startTime, durationMinutes),
    source: 'Staff' as BookingSource,
    rescheduleCount: 0,
    originalAppointmentId: null,
    consultationType: 'General Checkup',
    ...overrides,
  }
}

// Helper: get current week dates (Sun-Sat) relative to today
function getCurrentWeekDate(dayOffset: number, hour: number, minute: number = 0): { date: string; startTime: string } {
  const now = new Date()
  const sunday = new Date(now)
  sunday.setDate(now.getDate() - now.getDay())
  sunday.setHours(hour, minute, 0, 0)
  sunday.setDate(sunday.getDate() + dayOffset)
  const date = `${sunday.getFullYear()}-${String(sunday.getMonth() + 1).padStart(2, '0')}-${String(sunday.getDate()).padStart(2, '0')}`
  const startTime = `${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}:00`
  return { date, startTime }
}

const MOCK_APPOINTMENTS: CalendarAppointment[] = [
  mockAppointment(1, {
    animalName: 'Max', ownerName: 'Ahmed Al-Rashid',
    veterinarianId: MOCK_VETS[0].id, veterinarianName: MOCK_VETS[0].name, status: 'Scheduled',
    ...getCurrentWeekDate(0, 9, 0),
    reason: 'Annual vaccination',
    consultationType: 'Vaccination', durationMinutes: 30,
  }),
  mockAppointment(2, {
    animalName: 'Luna', ownerName: 'Fatima Hassan',
    veterinarianId: MOCK_VETS[1].id, veterinarianName: MOCK_VETS[1].name, status: 'CheckedIn',
    ...getCurrentWeekDate(1, 10, 30),
    reason: 'Skin condition follow-up',
    consultationType: 'Dermatology', durationMinutes: 45,
  }),
  mockAppointment(3, {
    animalName: 'Rocky', ownerName: 'Mohammed Al-Zaabi',
    veterinarianId: MOCK_VETS[0].id, veterinarianName: MOCK_VETS[0].name, status: 'InProgress',
    ...getCurrentWeekDate(1, 14, 0),
    reason: 'Post-surgery check',
    consultationType: 'Follow-up', durationMinutes: 30,
  }),
  mockAppointment(4, {
    animalName: 'Mango', ownerName: 'Noura Al-Ketbi',
    veterinarianId: MOCK_VETS[2].id, veterinarianName: MOCK_VETS[2].name, status: 'Completed',
    ...getCurrentWeekDate(2, 8, 30),
    reason: 'Feather loss examination',
    consultationType: 'Exotic Animal', durationMinutes: 60,
  }),
  mockAppointment(5, {
    animalName: 'Oreo', ownerName: 'Saeed Al-Hamdan',
    veterinarianId: MOCK_VETS[3].id, veterinarianName: MOCK_VETS[3].name, status: 'Cancelled',
    ...getCurrentWeekDate(3, 14, 0),
    reason: 'Routine check',
    consultationType: 'General Checkup', durationMinutes: 30,
  }),
  mockAppointment(6, {
    animalName: 'Sultan', ownerName: 'Hamdan Al-Maktoum',
    veterinarianId: MOCK_VETS[1].id, veterinarianName: MOCK_VETS[1].name, status: 'Scheduled',
    ...getCurrentWeekDate(4, 7, 0),
    reason: 'Dental examination',
    consultationType: 'Dental', durationMinutes: 90,
  }),
  mockAppointment(7, {
    id: 'apt-0000-0000-0000-000000000007',
    animalName: 'Cleo', ownerName: 'Mariam Al-Suwaidi',
    veterinarianId: MOCK_VETS[2].id, veterinarianName: MOCK_VETS[2].name, status: 'Scheduled',
    ...getCurrentWeekDate(0, 11, 0),
    reason: 'Emergency vomiting',
    consultationType: 'Emergency', durationMinutes: 45,
  }),
  mockAppointment(8, {
    id: 'apt-0000-0000-0000-000000000008',
    animalName: 'Buddy', ownerName: 'Rashid Al-Mualla',
    veterinarianId: MOCK_VETS[0].id, veterinarianName: MOCK_VETS[0].name, status: 'Scheduled',
    ...getCurrentWeekDate(2, 15, 30),
    reason: 'Teeth cleaning',
    consultationType: 'Grooming', durationMinutes: 60,
  }),
  mockAppointment(9, {
    id: 'apt-0000-0000-0000-000000000009',
    animalName: 'Simba', ownerName: 'Aisha Al-Falasi',
    veterinarianId: MOCK_VETS[3].id, veterinarianName: MOCK_VETS[3].name, status: 'Scheduled',
    ...getCurrentWeekDate(3, 9, 0),
    reason: 'Blood work and X-ray',
    consultationType: 'Laboratory / Diagnostics', durationMinutes: 60,
  }),
  mockAppointment(10, {
    id: 'apt-0000-0000-0000-00000000000a',
    animalName: 'Kira', ownerName: 'Yousef Al-Hashimi',
    veterinarianId: MOCK_VETS[1].id, veterinarianName: MOCK_VETS[1].name, status: 'Scheduled',
    ...getCurrentWeekDate(4, 10, 0),
    reason: 'Spay surgery',
    consultationType: 'Surgery', durationMinutes: 120,
  }),
]

const STATUS_TRANSITIONS: Record<string, string> = {
  CHECK_IN: 'CheckedIn',
  START: 'InProgress',
  COMPLETE: 'Completed',
  CANCEL: 'Cancelled',
}

export const appointmentHandlers = [
  // GET /api/vets
  http.get('/api/vets', () => {
    return HttpResponse.json<VetDto[]>(MOCK_VETS)
  }),

  // GET /api/appointments
  http.get('/api/appointments', ({ request }) => {
    const url = new URL(request.url)
    const status = url.searchParams.get('status')
    const veterinarianId = url.searchParams.get('veterinarianId')
    const date = url.searchParams.get('date')
    const page = Number.parseInt(url.searchParams.get('page') ?? '1', 10)
    const pageSize = Number.parseInt(url.searchParams.get('pageSize') ?? '10', 10)

    let items = [...MOCK_APPOINTMENTS]

    if (status) {
      items = items.filter((apt) => apt.status === status)
    }
    if (veterinarianId) {
      items = items.filter((apt) => apt.veterinarianId === veterinarianId)
    }
    if (date) {
      items = items.filter((apt) => apt.date === date)
    }

    const start = (page - 1) * pageSize
    const paged = items.slice(start, start + pageSize)

    return HttpResponse.json<PagedResult<CalendarAppointment>>({
      items: paged,
      totalCount: items.length,
      page,
      pageSize,
    })
  }),

  // GET /api/appointments/:id
  http.get('/api/appointments/:id', ({ params }) => {
    const appointment = MOCK_APPOINTMENTS.find((apt) => apt.id === params.id)
    if (!appointment) return new HttpResponse(null, { status: 404 })
    return HttpResponse.json(appointment)
  }),

  // POST /api/appointments
  http.post('/api/appointments', async ({ request }) => {
    const body = await request.json() as {
      animalId: string
      veterinarianId: string
      date: string
      startTime: string
      durationMinutes: number
      reason?: string
      source: BookingSource
    }

    const vet = MOCK_VETS.find((v) => v.id === body.veterinarianId)
    const newAppointment: CalendarAppointment = {
      id: crypto.randomUUID(),
      animalId: body.animalId,
      animalName: 'New Patient',
      ownerName: 'Unknown Owner',
      veterinarianId: body.veterinarianId,
      veterinarianName: vet?.name ?? 'Unknown Vet',
      status: 'Scheduled',
      date: body.date,
      startTime: body.startTime,
      durationMinutes: body.durationMinutes,
      endTime: computeEndTime(body.startTime, body.durationMinutes),
      reason: body.reason ?? null,
      clinicId: CLINIC_ID,
      source: body.source,
      rescheduleCount: 0,
      originalAppointmentId: null,
      consultationType: 'General Checkup',
    }

    MOCK_APPOINTMENTS.push(newAppointment)
    return HttpResponse.json(newAppointment, { status: 201 })
  }),

  // PATCH /api/appointments/:id/transition
  http.patch('/api/appointments/:id/transition', async ({ params, request }) => {
    const appointment = MOCK_APPOINTMENTS.find((apt) => apt.id === params.id)
    if (!appointment) return new HttpResponse(null, { status: 404 })

    const body = await request.json() as { action: string; reason?: string }
    const newStatus = STATUS_TRANSITIONS[body.action]

    if (!newStatus) {
      return HttpResponse.json({ title: `Unknown action: ${body.action}` }, { status: 422 })
    }

    appointment.status = newStatus as AppointmentDto['status']

    return HttpResponse.json(appointment)
  }),
]
