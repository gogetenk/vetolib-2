import { http, HttpResponse, delay } from 'msw'
import {
  MOCK_CONSULTATION_TYPES,
  MOCK_VETERINARIANS,
  MOCK_BOOKING_APPOINTMENTS,
} from '@/mocks/data/booking'
import type {
  BookingAppointmentDto,
  CreateBookingRequest,
  CancelBookingRequest,
  RescheduleBookingRequest,
  TimeSlotDto,
  AvailabilityResponseDto,
  SlotSuggestionDto,
} from '@/lib/api/booking-types'

const BASE = '/api/v1/portal/booking'

// Mutable copy so mutations persist within the session
const appointments: BookingAppointmentDto[] = MOCK_BOOKING_APPOINTMENTS.map(a => ({ ...a }))

// ─── UAE work week: 0=Sun, 1=Mon, 2=Tue, 3=Wed, 4=Thu ─────────────────────────
const UAE_WORK_DAYS = new Set([0, 1, 2, 3, 4])
const CLINIC_OPEN_HOUR = 8
const CLINIC_CLOSE_HOUR = 20
const SLOT_DURATION_MINUTES = 30

// ─── Auth helpers ──────────────────────────────────────────────────────────────

function getMagicToken(request: Request): string | null {
  const auth = request.headers.get('Authorization')
  if (!auth) return null
  const parts = auth.split(' ')
  return parts[1] ?? null
}

// ─── Slot generation helpers ───────────────────────────────────────────────────

function generateSlotsForDate(dateStr: string): TimeSlotDto[] {
  const date = new Date(dateStr)
  const dayOfWeek = date.getDay()

  // Only generate slots for UAE work days (Sun-Thu)
  if (!UAE_WORK_DAYS.has(dayOfWeek)) return []

  const slots: TimeSlotDto[] = []
  const vets = MOCK_VETERINARIANS

  for (let hour = CLINIC_OPEN_HOUR; hour < CLINIC_CLOSE_HOUR; hour++) {
    for (let minute = 0; minute < 60; minute += SLOT_DURATION_MINUTES) {
      const start = new Date(date)
      start.setHours(hour, minute, 0, 0)
      const end = new Date(start)
      end.setMinutes(end.getMinutes() + SLOT_DURATION_MINUTES)

      // Rotate vets across slots for realistic spread
      const slotIndex = (hour - CLINIC_OPEN_HOUR) * 2 + minute / SLOT_DURATION_MINUTES
      const vet = vets[slotIndex % vets.length]

      slots.push({
        startTime: start.toISOString(),
        endTime: end.toISOString(),
        veterinarianId: vet.id,
        veterinarianName: vet.name,
        isAvailable: true,
      })
    }
  }

  // Mark slots occupied by existing appointments as unavailable
  for (const appt of appointments) {
    if (appt.status === 'Scheduled') {
      const apptStart = new Date(appt.startTime).toISOString()
      const slot = slots.find(s => s.startTime === apptStart && s.veterinarianId === appt.veterinarianId)
      if (slot) slot.isAvailable = false
    }
  }

  return slots
}

// ─── Handlers ─────────────────────────────────────────────────────────────────

export const bookingHandlers = [
  // GET /api/v1/portal/booking/consultation-types — PUBLIC
  http.get(`${BASE}/consultation-types`, async () => {
    await delay(100)
    return HttpResponse.json(MOCK_CONSULTATION_TYPES)
  }),

  // GET /api/v1/portal/booking/veterinarians — PUBLIC
  http.get(`${BASE}/veterinarians`, async () => {
    await delay(100)
    return HttpResponse.json(MOCK_VETERINARIANS)
  }),

  // GET /api/v1/portal/booking/availability — PUBLIC
  http.get(`${BASE}/availability`, async ({ request }) => {
    await delay(150)
    const url = new URL(request.url)
    const date = url.searchParams.get('date')

    if (!date) {
      return HttpResponse.json(
        { title: 'date query parameter is required (YYYY-MM-DD).' },
        { status: 400 }
      )
    }

    const slots = generateSlotsForDate(date)

    // Filter by veterinarianId if provided
    const vetId = url.searchParams.get('veterinarianId')
    const filtered = vetId ? slots.filter(s => s.veterinarianId === vetId) : slots

    const response: AvailabilityResponseDto = { date, slots: filtered }
    return HttpResponse.json(response)
  }),

  // POST /api/v1/portal/booking/suggest-slot — PUBLIC
  http.post(`${BASE}/suggest-slot`, async ({ request }) => {
    await delay(200)
    const body = await request.json() as {
      consultationTypeId: string
      preferredDates: string[]
      veterinarianId?: string
    }

    const preferredVetId = body.veterinarianId ?? MOCK_VETERINARIANS[0].id
    const vet = MOCK_VETERINARIANS.find(v => v.id === preferredVetId) ?? MOCK_VETERINARIANS[0]

    const suggestions: SlotSuggestionDto[] = []
    const d = new Date()
    d.setDate(d.getDate() + 1) // start tomorrow
    let checked = 0

    while (suggestions.length < 3 && checked < 30) {
      if (UAE_WORK_DAYS.has(d.getDay())) {
        const hour = 9 + suggestions.length * 2 // 09:00, 11:00, 13:00
        const start = new Date(d)
        start.setHours(hour, 0, 0, 0)
        const end = new Date(start)
        end.setMinutes(end.getMinutes() + 30)

        suggestions.push({
          startTime: start.toISOString(),
          endTime: end.toISOString(),
          veterinarianId: vet.id,
          veterinarianName: vet.name,
          score: 0.9 - suggestions.length * 0.1,
        })
      }
      d.setDate(d.getDate() + 1)
      checked++
    }

    return HttpResponse.json(suggestions)
  }),

  // POST /api/v1/portal/booking/appointments — MagicLink auth required
  http.post(`${BASE}/appointments`, async ({ request }) => {
    await delay(300)
    const token = getMagicToken(request)
    if (!token) {
      return HttpResponse.json(
        { title: 'Authentication required. Please use your booking link.' },
        { status: 401 }
      )
    }
    if (token === 'expired-magic-token') {
      return HttpResponse.json(
        { title: 'This link has expired. Please contact your clinic to receive a new one.' },
        { status: 401 }
      )
    }

    const body = await request.json() as CreateBookingRequest

    if (!body.consultationTypeId || !body.veterinarianId || !body.startTime || !body.ownerName || !body.ownerEmail) {
      return HttpResponse.json(
        { title: 'Missing required fields: consultationTypeId, veterinarianId, startTime, ownerName, ownerEmail.' },
        { status: 422 }
      )
    }

    const consultationType = MOCK_CONSULTATION_TYPES.find(c => c.id === body.consultationTypeId)
    if (!consultationType) {
      return HttpResponse.json(
        { title: 'Consultation type not found.' },
        { status: 404 }
      )
    }

    const vet = MOCK_VETERINARIANS.find(v => v.id === body.veterinarianId)
    if (!vet) {
      return HttpResponse.json(
        { title: 'Veterinarian not found.' },
        { status: 404 }
      )
    }

    const startDate = new Date(body.startTime)
    const endDate = new Date(startDate)
    endDate.setMinutes(endDate.getMinutes() + consultationType.durationMinutes)

    const newAppt: BookingAppointmentDto = {
      id: crypto.randomUUID(),
      consultationTypeId: body.consultationTypeId,
      consultationTypeName: consultationType.name,
      veterinarianId: body.veterinarianId,
      veterinarianName: vet.name,
      startTime: startDate.toISOString(),
      endTime: endDate.toISOString(),
      petName: body.petName,
      petSpecies: body.petSpecies,
      ownerName: body.ownerName,
      ownerPhone: body.ownerPhone,
      ownerEmail: body.ownerEmail,
      notes: body.notes ?? null,
      status: 'Scheduled',
      rescheduleCount: 0,
      createdAt: new Date().toISOString(),
    }

    appointments.push(newAppt)
    return HttpResponse.json(newAppt, { status: 201 })
  }),

  // GET /api/v1/portal/booking/appointments — MagicLink auth required
  http.get(`${BASE}/appointments`, async ({ request }) => {
    await delay(150)
    const token = getMagicToken(request)
    if (!token) {
      return HttpResponse.json(
        { title: 'Authentication required. Please use your booking link.' },
        { status: 401 }
      )
    }
    if (token === 'expired-magic-token') {
      return HttpResponse.json(
        { title: 'This link has expired. Please contact your clinic to receive a new one.' },
        { status: 401 }
      )
    }

    const url = new URL(request.url)
    const email = url.searchParams.get('ownerEmail')

    const result = email
      ? appointments.filter(a => a.ownerEmail === email)
      : appointments

    return HttpResponse.json(result)
  }),

  // GET /api/v1/portal/booking/appointments/:id — MagicLink auth required
  http.get(`${BASE}/appointments/:id`, async ({ params, request }) => {
    await delay(100)
    const token = getMagicToken(request)
    if (!token) {
      return HttpResponse.json(
        { title: 'Authentication required. Please use your booking link.' },
        { status: 401 }
      )
    }
    if (token === 'expired-magic-token') {
      return HttpResponse.json(
        { title: 'This link has expired. Please contact your clinic to receive a new one.' },
        { status: 401 }
      )
    }

    const appt = appointments.find(a => a.id === params.id)
    if (!appt) {
      return HttpResponse.json(
        { title: 'Appointment not found.' },
        { status: 404 }
      )
    }

    return HttpResponse.json(appt)
  }),

  // POST /api/v1/portal/booking/appointments/:id/cancel — MagicLink, 24h rule
  http.post(`${BASE}/appointments/:id/cancel`, async ({ params, request }) => {
    await delay(200)
    const token = getMagicToken(request)
    if (!token) {
      return HttpResponse.json(
        { title: 'Authentication required. Please use your booking link.' },
        { status: 401 }
      )
    }
    if (token === 'expired-magic-token') {
      return HttpResponse.json(
        { title: 'This link has expired. Please contact your clinic to receive a new one.' },
        { status: 401 }
      )
    }

    const appt = appointments.find(a => a.id === params.id)
    if (!appt) {
      return HttpResponse.json(
        { title: 'Appointment not found.' },
        { status: 404 }
      )
    }

    if (appt.status !== 'Scheduled') {
      return HttpResponse.json(
        { title: 'Only scheduled appointments can be cancelled.' },
        { status: 422 }
      )
    }

    // 24-hour cancellation rule
    const hoursUntilAppt = (new Date(appt.startTime).getTime() - Date.now()) / (1000 * 60 * 60)
    if (hoursUntilAppt < 24) {
      return HttpResponse.json(
        { title: 'Appointments cannot be cancelled less than 24 hours before the scheduled time.' },
        { status: 422 }
      )
    }

    const body = await request.json().catch(() => ({})) as CancelBookingRequest
    appt.status = 'Cancelled'

    return HttpResponse.json({ ...appt, cancelReason: body.reason ?? null })
  }),

  // POST /api/v1/portal/booking/appointments/:id/reschedule — MagicLink, max 2 reschedules
  http.post(`${BASE}/appointments/:id/reschedule`, async ({ params, request }) => {
    await delay(250)
    const token = getMagicToken(request)
    if (!token) {
      return HttpResponse.json(
        { title: 'Authentication required. Please use your booking link.' },
        { status: 401 }
      )
    }
    if (token === 'expired-magic-token') {
      return HttpResponse.json(
        { title: 'This link has expired. Please contact your clinic to receive a new one.' },
        { status: 401 }
      )
    }

    const appt = appointments.find(a => a.id === params.id)
    if (!appt) {
      return HttpResponse.json(
        { title: 'Appointment not found.' },
        { status: 404 }
      )
    }

    if (appt.status !== 'Scheduled') {
      return HttpResponse.json(
        { title: 'Only scheduled appointments can be rescheduled.' },
        { status: 422 }
      )
    }

    if (appt.rescheduleCount >= 2) {
      return HttpResponse.json(
        { title: 'This appointment has already been rescheduled the maximum number of times (2). Please contact the clinic directly.' },
        { status: 422 }
      )
    }

    const body = await request.json() as RescheduleBookingRequest
    if (!body.newStartTime) {
      return HttpResponse.json(
        { title: 'newStartTime is required.' },
        { status: 422 }
      )
    }

    const consultationType = MOCK_CONSULTATION_TYPES.find(c => c.id === appt.consultationTypeId)
    const durationMinutes = consultationType?.durationMinutes ?? 30

    const newStart = new Date(body.newStartTime)
    const newEnd = new Date(newStart)
    newEnd.setMinutes(newEnd.getMinutes() + durationMinutes)

    appt.startTime = newStart.toISOString()
    appt.endTime = newEnd.toISOString()
    appt.rescheduleCount += 1

    return HttpResponse.json(appt)
  }),
]
