import { http, HttpResponse, delay } from 'msw'
import {
  MOCK_CONSULTATION_TYPES,
  MOCK_VETERINARIANS,
  MOCK_BOOKINGS,
} from '@/mocks/data/booking'
import type { BookingAppointmentDto, DayAvailabilityDto } from '@/lib/api/booking-types'

const BASE = '/api/v1/portal'

function getOwnerToken(request: Request): string | null {
  const auth = request.headers.get('Authorization')
  if (!auth) return null
  const parts = auth.split(' ')
  return parts[1] ?? null
}

// Mutable copy so created bookings persist within session
const bookings: BookingAppointmentDto[] = MOCK_BOOKINGS.map(b => ({ ...b }))

// Simulate a slot-unavailable scenario for one specific slot
const UNAVAILABLE_SLOT = { date: '2026-03-12', startTime: '11:00' }

function generateSlots(date: string, durationMinutes: number): DayAvailabilityDto {
  // UAE working hours 8:00 - 18:00, slots every durationMinutes
  const slots = []
  let hour = 8
  let minute = 0

  while (hour < 18) {
    const startTime = `${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}`
    const totalMinutes = hour * 60 + minute + durationMinutes
    const endHour = Math.floor(totalMinutes / 60)
    const endMinute = totalMinutes % 60
    const endTime = `${String(endHour).padStart(2, '0')}:${String(endMinute).padStart(2, '0')}`

    if (endHour > 18 || (endHour === 18 && endMinute > 0)) break

    // Mark lunchtime slots (13:00-14:00) as unavailable
    const isLunch = hour === 13
    // Mark the specific test-unavailable slot
    const isUnavailableSlot = date === UNAVAILABLE_SLOT.date && startTime === UNAVAILABLE_SLOT.startTime

    slots.push({
      startTime,
      endTime,
      isAvailable: !isLunch && !isUnavailableSlot,
    })

    minute += durationMinutes
    if (minute >= 60) {
      hour += Math.floor(minute / 60)
      minute = minute % 60
    }
  }

  return { date, slots }
}

export const bookingHandlers = [
  // GET /api/v1/portal/clinics/:clinicSlug/consultation-types
  http.get(`${BASE}/clinics/:clinicSlug/consultation-types`, async ({ request }) => {
    await delay(150)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    return HttpResponse.json(MOCK_CONSULTATION_TYPES)
  }),

  // GET /api/v1/portal/clinics/:clinicSlug/veterinarians
  http.get(`${BASE}/clinics/:clinicSlug/veterinarians`, async ({ request }) => {
    await delay(100)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    return HttpResponse.json(MOCK_VETERINARIANS)
  }),

  // GET /api/v1/portal/clinics/:clinicSlug/availability
  http.get(`${BASE}/clinics/:clinicSlug/availability`, async ({ request }) => {
    await delay(200)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    const url = new URL(request.url)
    const date = url.searchParams.get('date') ?? new Date().toISOString().split('T')[0]
    const consultationTypeId = url.searchParams.get('consultationTypeId') ?? 'ct-001'

    const consultationType = MOCK_CONSULTATION_TYPES.find(ct => ct.id === consultationTypeId)
    const duration = consultationType?.durationMinutes ?? 30

    const availability = generateSlots(date, duration)
    return HttpResponse.json(availability)
  }),

  // POST /api/v1/portal/bookings
  http.post(`${BASE}/bookings`, async ({ request }) => {
    await delay(300)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    const body = await request.json() as {
      consultationTypeId: string
      veterinarianId: string | null
      date: string
      startTime: string
      animalId: string
      animalName: string
      reason: string
    }

    // Simulate slot unavailable scenario
    if (body.date === UNAVAILABLE_SLOT.date && body.startTime === UNAVAILABLE_SLOT.startTime) {
      return HttpResponse.json(
        { title: 'This slot is no longer available. Please choose a different time.' },
        { status: 409 }
      )
    }

    const consultationType = MOCK_CONSULTATION_TYPES.find(ct => ct.id === body.consultationTypeId)
    const vet = body.veterinarianId
      ? MOCK_VETERINARIANS.find(v => v.id === body.veterinarianId)
      : MOCK_VETERINARIANS[0]

    const durationMinutes = consultationType?.durationMinutes ?? 30
    const [startHour, startMin] = body.startTime.split(':').map(Number)
    const endTotal = startHour * 60 + startMin + durationMinutes
    const endTime = `${String(Math.floor(endTotal / 60)).padStart(2, '0')}:${String(endTotal % 60).padStart(2, '0')}`

    const newBooking: BookingAppointmentDto = {
      id: crypto.randomUUID(),
      date: body.date,
      startTime: body.startTime,
      endTime,
      animalName: body.animalName,
      consultationTypeName: consultationType?.name ?? 'Consultation',
      veterinarianName: vet?.fullName ?? 'Dr. Unknown',
      clinicName: 'Desert Paws Clinic',
      clinicAddress: 'Sheikh Zayed Road, Dubai, UAE',
      status: 'Scheduled',
      notes: body.reason || null,
    }

    bookings.unshift(newBooking)
    return HttpResponse.json(newBooking, { status: 201 })
  }),

  // GET /api/v1/portal/bookings
  http.get(`${BASE}/bookings`, async ({ request }) => {
    await delay(150)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    return HttpResponse.json(bookings)
  }),
]
