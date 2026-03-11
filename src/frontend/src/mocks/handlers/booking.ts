import { http, HttpResponse, delay } from 'msw'
import { getMockOwnerAppointments } from '@/mocks/data/booking'
import type {
  BookingAppointmentDto,
  CancelBookingAppointmentRequest,
  RescheduleBookingAppointmentRequest,
} from '@/lib/api/booking-types'

const BASE = '/api/v1/portal/booking'

// Mutable in-memory state
let ownerAppointments: BookingAppointmentDto[] = getMockOwnerAppointments()

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
