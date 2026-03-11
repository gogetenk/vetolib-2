import { http, HttpResponse } from 'msw'
import type { AppointmentDto, VetDto, PagedResult } from '@/lib/api/appointments'

const MOCK_VETS: VetDto[] = [
  { id: 'vet-0000-0000-0000-000000000001', name: 'Dr. Sarah Johnson' },
  { id: 'vet-0000-0000-0000-000000000002', name: 'Dr. Omar Al-Rashid' },
  { id: 'vet-0000-0000-0000-000000000003', name: 'Dr. Layla Al-Mansoori' },
  { id: 'vet-0000-0000-0000-000000000004', name: 'Dr. Khalid Ibrahim' },
]

const MOCK_APPOINTMENTS: AppointmentDto[] = [
  {
    id: 'apt-0000-0000-0000-000000000001',
    patientName: 'Max',
    species: 'Dog',
    ownerName: 'Ahmed Al-Rashid',
    ownerPhone: '+971 50 123 4567',
    vetId: 'vet-0000-0000-0000-000000000001',
    vetName: 'Dr. Sarah Johnson',
    status: 'SCHEDULED',
    scheduledAt: new Date('2026-03-12T09:00:00+04:00').toISOString(),
    reason: 'Annual vaccination',
    notes: 'Owner requested morning slot',
    clinicId: 'clinic-001',
  },
  {
    id: 'apt-0000-0000-0000-000000000002',
    patientName: 'Luna',
    species: 'Cat',
    ownerName: 'Fatima Hassan',
    ownerPhone: '+971 55 987 6543',
    vetId: 'vet-0000-0000-0000-000000000002',
    vetName: 'Dr. Omar Al-Rashid',
    status: 'CHECKED_IN',
    scheduledAt: new Date('2026-03-11T10:30:00+04:00').toISOString(),
    reason: 'Skin condition follow-up',
    clinicId: 'clinic-001',
  },
  {
    id: 'apt-0000-0000-0000-000000000003',
    patientName: 'Rocky',
    species: 'Dog',
    ownerName: 'Mohammed Al-Zaabi',
    ownerPhone: '+971 54 321 0987',
    vetId: 'vet-0000-0000-0000-000000000001',
    vetName: 'Dr. Sarah Johnson',
    status: 'IN_PROGRESS',
    scheduledAt: new Date('2026-03-11T11:00:00+04:00').toISOString(),
    reason: 'Post-surgery check',
    notes: 'Please prepare examination room 2',
    clinicId: 'clinic-001',
  },
  {
    id: 'apt-0000-0000-0000-000000000004',
    patientName: 'Mango',
    species: 'Bird',
    ownerName: 'Noura Al-Ketbi',
    ownerPhone: '+971 56 456 7890',
    vetId: 'vet-0000-0000-0000-000000000003',
    vetName: 'Dr. Layla Al-Mansoori',
    status: 'COMPLETED',
    scheduledAt: new Date('2026-03-10T08:30:00+04:00').toISOString(),
    reason: 'Feather loss examination',
    clinicId: 'clinic-001',
  },
  {
    id: 'apt-0000-0000-0000-000000000005',
    patientName: 'Oreo',
    species: 'Rabbit',
    ownerName: 'Saeed Al-Hamdan',
    ownerPhone: '+971 50 789 0123',
    vetId: 'vet-0000-0000-0000-000000000004',
    vetName: 'Dr. Khalid Ibrahim',
    status: 'CANCELLED',
    scheduledAt: new Date('2026-03-09T14:00:00+04:00').toISOString(),
    reason: 'Routine check',
    cancellationReason: 'Owner request — travel',
    clinicId: 'clinic-001',
  },
  {
    id: 'apt-0000-0000-0000-000000000006',
    patientName: 'Sultan',
    species: 'Horse',
    ownerName: 'Hamdan Al-Maktoum',
    ownerPhone: '+971 52 111 2233',
    vetId: 'vet-0000-0000-0000-000000000002',
    vetName: 'Dr. Omar Al-Rashid',
    status: 'SCHEDULED',
    scheduledAt: new Date('2026-03-13T07:00:00+04:00').toISOString(),
    reason: 'Dental examination',
    notes: 'Large animal — book extended slot',
    clinicId: 'clinic-001',
  },
]

const STATUS_TRANSITIONS: Record<string, string> = {
  CHECK_IN: 'CHECKED_IN',
  START: 'IN_PROGRESS',
  COMPLETE: 'COMPLETED',
  CANCEL: 'CANCELLED',
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
    const vetId = url.searchParams.get('vetId')
    const date = url.searchParams.get('date')
    const page = parseInt(url.searchParams.get('page') ?? '1')
    const pageSize = parseInt(url.searchParams.get('pageSize') ?? '10')

    let items = [...MOCK_APPOINTMENTS]

    if (status) {
      items = items.filter((apt) => apt.status === status)
    }
    if (vetId) {
      items = items.filter((apt) => apt.vetId === vetId)
    }
    if (date) {
      items = items.filter((apt) => apt.scheduledAt.startsWith(date))
    }

    const start = (page - 1) * pageSize
    const paged = items.slice(start, start + pageSize)

    return HttpResponse.json<PagedResult<AppointmentDto>>({
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
      patientName: string
      species: string
      ownerName: string
      ownerPhone: string
      vetId: string
      scheduledAt: string
      reason: string
      notes?: string
    }

    const vet = MOCK_VETS.find((v) => v.id === body.vetId)
    const newAppointment: AppointmentDto = {
      id: crypto.randomUUID(),
      patientName: body.patientName,
      species: body.species as AppointmentDto['species'],
      ownerName: body.ownerName,
      ownerPhone: body.ownerPhone,
      vetId: body.vetId,
      vetName: vet?.name ?? 'Unknown Vet',
      status: 'SCHEDULED',
      scheduledAt: body.scheduledAt,
      reason: body.reason,
      notes: body.notes,
      clinicId: 'clinic-001',
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
    if (body.action === 'CANCEL' && body.reason) {
      appointment.cancellationReason = body.reason
    }

    return HttpResponse.json(appointment)
  }),
]
