import { http, HttpResponse, delay } from 'msw'
import { getMockOwnerAppointments, MOCK_CONSULTATION_TYPES, MOCK_VETERINARIANS } from '@/mocks/data/booking'
import type {
  BookingAppointmentDto,
  CancelBookingAppointmentRequest,
  RescheduleBookingAppointmentRequest,
  AvailabilityDayDto,
} from '@/lib/api/booking-types'

const BASE = '/api/v1/portal/booking'

// ─── Helpers ──────────────────────────────────────────────────────────────────

const UAE_WORK_DAYS = new Set([0, 1, 2, 3, 4]) // Sun=0 … Thu=4

function generateAvailabilitySlots(date: string, durationMinutes: number): AvailabilityDayDto {
  const d = new Date(date + 'T00:00:00')
  const isWorkDay = UAE_WORK_DAYS.has(d.getDay())
  if (!isWorkDay) return { date, slots: [] }

  const slots = []
  let hour = 8
  let minute = 0

  while (hour < 18) {
    const startTime = `${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}`
    const totalEnd = hour * 60 + minute + durationMinutes
    const endHour = Math.floor(totalEnd / 60)
    const endMinute = totalEnd % 60
    if (endHour > 18 || (endHour === 18 && endMinute > 0)) break

    const isLunch = hour === 13
    slots.push({
      time: startTime,
      isAvailable: !isLunch,
    })

    minute += durationMinutes
    if (minute >= 60) {
      hour += Math.floor(minute / 60)
      minute = minute % 60
    }
  }

  return { date, slots }
}

// Mutable in-memory state
const ownerAppointments: BookingAppointmentDto[] = getMockOwnerAppointments()

// ─── Auth helper ──────────────────────────────────────────────────────────────

function requireAuth(request: Request): Response | null {
  const auth = request.headers.get('Authorization')
  if (!auth) {
    return HttpResponse.json(
      { title: 'Authentication required. Please use your magic link to access this resource.' },
      { status: 401 }
    )
  }
  const token = auth.split(' ')[1]
  if (token === 'expired-magic-token') {
    return HttpResponse.json(
      { title: 'This link has expired. Please contact your clinic to receive a new one.' },
      { status: 401 }
    )
  }
  return null
}

// ─── Handlers ─────────────────────────────────────────────────────────────────

export const bookingHandlers = [
  // GET /api/v1/portal/booking/clinics/:clinicSlug/consultation-types
  http.get(`${BASE}/clinics/:clinicSlug/consultation-types`, async ({ request }) => {
    await delay(150)
    const authError = requireAuth(request)
    if (authError) return authError

    return HttpResponse.json(MOCK_CONSULTATION_TYPES)
  }),

  // GET /api/v1/portal/booking/clinics/:clinicSlug/veterinarians
  http.get(`${BASE}/clinics/:clinicSlug/veterinarians`, async ({ request }) => {
    await delay(100)
    const authError = requireAuth(request)
    if (authError) return authError

    return HttpResponse.json(MOCK_VETERINARIANS)
  }),

  // GET /api/v1/portal/booking/clinics/:clinicSlug/availability
  http.get(`${BASE}/clinics/:clinicSlug/availability`, async ({ request }) => {
    await delay(200)
    const authError = requireAuth(request)
    if (authError) return authError

    const url = new URL(request.url)
    const date = url.searchParams.get('date') ?? new Date().toISOString().split('T')[0]
    const consultationTypeId = url.searchParams.get('consultationTypeId') ?? ''

    const type = MOCK_CONSULTATION_TYPES.find(ct => ct.id === consultationTypeId)
    const duration = type?.durationMinutes ?? 30

    return HttpResponse.json(generateAvailabilitySlots(date, duration))
  }),

  // GET /api/v1/portal/booking/appointments
  http.get(`${BASE}/appointments`, async ({ request }) => {
    await delay(150)
    const authError = requireAuth(request)
    if (authError) return authError

    return HttpResponse.json(ownerAppointments)
  }),

  // GET /api/v1/portal/booking/appointments/:id
  http.get(`${BASE}/appointments/:id`, async ({ params, request }) => {
    await delay(100)
    const authError = requireAuth(request)
    if (authError) return authError

    const appt = ownerAppointments.find(a => a.id === params.id)
    if (!appt) return new HttpResponse(null, { status: 404 })

    return HttpResponse.json(appt)
  }),

  // POST /api/v1/portal/booking/appointments
  http.post(`${BASE}/appointments`, async ({ request }) => {
    await delay(250)
    const authError = requireAuth(request)
    if (authError) return authError

    const body = await request.json() as {
      consultationTypeId: string
      veterinarianId: string
      petName: string
      scheduledAt: string
      notes: string | null
    }

    const newAppt: BookingAppointmentDto = {
      id: crypto.randomUUID(),
      consultationTypeId: body.consultationTypeId,
      consultationTypeName: 'General Consultation',
      veterinarianId: body.veterinarianId,
      veterinarianName: 'Dr. Ahmed Al-Rashidi',
      petName: body.petName,
      scheduledAt: body.scheduledAt,
      durationMinutes: 30,
      status: 'Scheduled',
      notes: body.notes,
      clinicName: 'Desert Paws Veterinary Clinic',
      clinicAddress: 'Al Wasl Road, Jumeirah 1, Dubai, UAE',
    }

    ownerAppointments.unshift(newAppt)
    return HttpResponse.json(newAppt, { status: 201 })
  }),

  // POST /api/v1/portal/booking/appointments/:id/cancel
  http.post(`${BASE}/appointments/:id/cancel`, async ({ params, request }) => {
    await delay(200)
    const authError = requireAuth(request)
    if (authError) return authError

    const appt = ownerAppointments.find(a => a.id === params.id)
    if (!appt) return new HttpResponse(null, { status: 404 })

    if (appt.status !== 'Scheduled') {
      return HttpResponse.json(
        { title: 'Only scheduled appointments can be cancelled.' },
        { status: 422 }
      )
    }

    const scheduledAt = new Date(appt.scheduledAt).getTime()
    const hoursUntil = (scheduledAt - Date.now()) / (1000 * 60 * 60)
    if (hoursUntil < 24) {
      return HttpResponse.json(
        { title: 'Appointments can only be cancelled at least 24 hours in advance. Please contact the clinic directly.' },
        { status: 422 }
      )
    }

    const body = await request.json() as CancelBookingAppointmentRequest
    appt.status = 'Cancelled'
    if (body.reason) appt.notes = body.reason

    return HttpResponse.json(appt)
  }),

  // POST /api/v1/portal/booking/appointments/:id/reschedule
  http.post(`${BASE}/appointments/:id/reschedule`, async ({ params, request }) => {
    await delay(200)
    const authError = requireAuth(request)
    if (authError) return authError

    const appt = ownerAppointments.find(a => a.id === params.id)
    if (!appt) return new HttpResponse(null, { status: 404 })

    if (appt.status !== 'Scheduled') {
      return HttpResponse.json(
        { title: 'Only scheduled appointments can be rescheduled.' },
        { status: 422 }
      )
    }

    const body = await request.json() as RescheduleBookingAppointmentRequest
    appt.scheduledAt = body.newScheduledAt

    return HttpResponse.json(appt)
  }),
]
