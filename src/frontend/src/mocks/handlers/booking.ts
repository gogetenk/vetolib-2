import { http, HttpResponse, delay } from 'msw'
import type { BookingDay, BookingSlot, SlotSuggestionDto } from '@/lib/api/booking'
import type { ConsultationTypeDto, VeterinarianDto, BookingPetDto, CreateBookingAppointmentRequest } from '@/lib/api/booking'

const MOCK_VETS = [
  { id: 'vet-0000-0000-0000-000000000001', name: 'Dr. Sarah Johnson' },
  { id: 'vet-0000-0000-0000-000000000002', name: 'Dr. Omar Al-Rashid' },
  { id: 'vet-0000-0000-0000-000000000003', name: 'Dr. Layla Al-Mansoori' },
]

/** Fri/Sat are clinic closed days (UAE weekend) */
const CLOSED_DAYS = new Set([5, 6]) // 0=Sun, 5=Fri, 6=Sat

/** Generate 30-min slots from 08:00 to 17:30 Asia/Dubai */
function generateDaySlots(dateStr: string, vetId: string, vetName: string): BookingSlot[] {
  const slots: BookingSlot[] = []

  // Use Asia/Dubai offset (+04:00)
  const START_HOUR = 8
  const END_HOUR = 17
  const SLOT_MINUTES = 30

  // Deterministic "unavailable" slots based on date+time hash for realism
  const busySlots = new Set<number>([9, 10, 14, 15, 21]) // indices of blocked slots

  let idx = 0
  for (let hour = START_HOUR; hour <= END_HOUR; hour++) {
    for (let minute = 0; minute < 60; minute += SLOT_MINUTES) {
      if (hour === END_HOUR && minute > 30) break

      const padH = String(hour).padStart(2, '0')
      const padM = String(minute).padStart(2, '0')
      const endMinuteTotal = hour * 60 + minute + SLOT_MINUTES
      const endH = String(Math.floor(endMinuteTotal / 60)).padStart(2, '0')
      const endM = String(endMinuteTotal % 60).padStart(2, '0')

      slots.push({
        startsAt: `${dateStr}T${padH}:${padM}:00+04:00`,
        endsAt: `${dateStr}T${endH}:${endM}:00+04:00`,
        vetId,
        vetName,
        available: !busySlots.has(idx),
      })
      idx++
    }
  }

  return slots
}

/** Get 7 days starting from the given Sunday (ISO YYYY-MM-DD in Asia/Dubai) */
function buildWeekDays(weekStart: string, vetId?: string): BookingDay[] {
  const days: BookingDay[] = []

  // Parse weekStart as local date parts (no timezone conversion issues)
  const [year, month, day] = weekStart.split('-').map(Number)
  const base = new Date(Date.UTC(year, month - 1, day))

  for (let i = 0; i < 7; i++) {
    const d = new Date(base.getTime() + i * 86400000)
    const dow = d.getUTCDay() // 0=Sun
    const y = d.getUTCFullYear()
    const m = String(d.getUTCMonth() + 1).padStart(2, '0')
    const dd = String(d.getUTCDate()).padStart(2, '0')
    const dateStr = `${y}-${m}-${dd}`

    const closed = CLOSED_DAYS.has(dow)

    let slots: BookingSlot[] = []
    if (!closed) {
      if (vetId) {
        const vet = MOCK_VETS.find(v => v.id === vetId)
        if (vet) {
          slots = generateDaySlots(dateStr, vet.id, vet.name)
        }
      } else {
        // Show one vet's slots per day in rotation (for demo)
        const vet = MOCK_VETS[i % MOCK_VETS.length]
        slots = generateDaySlots(dateStr, vet.id, vet.name)
      }
    }

    days.push({ date: dateStr, slots, closed })
  }

  return days
}

// ─── Portal booking mock data ──────────────────────────────────────────────────

const MOCK_CONSULTATION_TYPES: ConsultationTypeDto[] = [
  { id: 'ct-001', name: 'General Checkup', durationMinutes: 30, description: 'Routine wellness examination' },
  { id: 'ct-002', name: 'Vaccination', durationMinutes: 20, description: 'Annual vaccine boosters' },
  { id: 'ct-003', name: 'Dental Care', durationMinutes: 60, description: 'Dental cleaning and examination' },
  { id: 'ct-004', name: 'Emergency', durationMinutes: 30, description: 'Urgent medical attention' },
  { id: 'ct-005', name: 'Lab Tests', durationMinutes: 15, description: 'Blood work and diagnostic tests' },
  { id: 'ct-006', name: 'Surgery Consultation', durationMinutes: 45, description: 'Pre- or post-surgical evaluation' },
]

const MOCK_VETERINARIANS: VeterinarianDto[] = [
  { id: 'vet-0000-0000-0000-000000000001', name: 'Dr. Sarah Johnson', specialties: ['Small Animals', 'Surgery'] },
  { id: 'vet-0000-0000-0000-000000000002', name: 'Dr. Omar Al-Rashid', specialties: ['Exotic Animals', 'Dermatology'] },
  { id: 'vet-0000-0000-0000-000000000003', name: 'Dr. Layla Al-Mansoori', specialties: ['Dentistry', 'Internal Medicine'] },
]

const MOCK_BOOKING_PETS: BookingPetDto[] = [
  { id: 'pet-0001', name: 'Zayed', species: 'Dog', breed: 'Labrador Retriever', ageYears: 3 },
  { id: 'pet-0002', name: 'Lulu', species: 'Cat', breed: 'Persian', ageYears: 5 },
  { id: 'pet-0003', name: 'Falcon', species: 'Bird', breed: 'Falcon — Saker', ageYears: 2 },
]

// Track created appointments in memory
const createdBookingAppointments: Array<{ id: string } & CreateBookingAppointmentRequest> = []

const PORTAL_BASE = '/api/v1/portal/booking'

function getPortalToken(request: Request): string | null {
  const auth = request.headers.get('Authorization')
  if (!auth) return null
  const parts = auth.split(' ')
  return parts[1] ?? null
}

const BASE = '/api/v1/booking'

export const bookingHandlers = [
  // GET /api/v1/booking/slots?weekStart=YYYY-MM-DD
  http.get(`${BASE}/slots`, async ({ request }) => {
    await delay(200)
    const url = new URL(request.url)
    const weekStart = url.searchParams.get('weekStart') ?? ''
    const vetId = url.searchParams.get('vetId') ?? undefined

    if (!weekStart || !/^\d{4}-\d{2}-\d{2}$/.test(weekStart)) {
      return HttpResponse.json({ title: 'weekStart is required (YYYY-MM-DD)' }, { status: 400 })
    }

    const days = buildWeekDays(weekStart, vetId)
    return HttpResponse.json(days)
  }),

  // GET /api/v1/booking/suggest?from=YYYY-MM-DD&to=YYYY-MM-DD
  http.get(`${BASE}/suggest`, async ({ request }) => {
    await delay(300)
    const url = new URL(request.url)
    const from = url.searchParams.get('from') ?? ''
    const vetId = url.searchParams.get('vetId') ?? undefined

    if (!from) {
      return HttpResponse.json({ title: 'from is required' }, { status: 400 })
    }

    // Build suggestions: pick 3 slots in the next few days
    const suggestions: SlotSuggestionDto[] = []
    const [year, month, day] = from.split('-').map(Number)
    const base = new Date(Date.UTC(year, month - 1, day))

    let found = 0
    for (let i = 0; i < 14 && found < 3; i++) {
      const d = new Date(base.getTime() + i * 86400000)
      const dow = d.getUTCDay()
      if (CLOSED_DAYS.has(dow)) continue

      const y = d.getUTCFullYear()
      const m = String(d.getUTCMonth() + 1).padStart(2, '0')
      const dd = String(d.getUTCDate()).padStart(2, '0')
      const dateStr = `${y}-${m}-${dd}`

      const vet = vetId
        ? MOCK_VETS.find(v => v.id === vetId) ?? MOCK_VETS[found % MOCK_VETS.length]
        : MOCK_VETS[found % MOCK_VETS.length]

      // Pick morning (09:00), midday (11:00), afternoon (14:30) for variety
      const times = ['09:00', '11:00', '14:30']
      const time = times[found % times.length]
      const [h, min] = time.split(':').map(Number)
      const endH = String(Math.floor((h * 60 + min + 30) / 60)).padStart(2, '0')
      const endM = String((h * 60 + min + 30) % 60).padStart(2, '0')

      suggestions.push({
        vetId: vet.id,
        vetName: vet.name,
        startsAt: `${dateStr}T${time}:00+04:00`,
        endsAt: `${dateStr}T${endH}:${endM}:00+04:00`,
        score: 0.95 - found * 0.1,
      })
      found++
    }

    return HttpResponse.json(suggestions)
  }),

  // ─── Portal booking endpoints ────────────────────────────────────────────────

  // GET /api/v1/portal/booking/consultation-types
  http.get(`${PORTAL_BASE}/consultation-types`, async ({ request }) => {
    await delay(150)
    const token = getPortalToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })
    return HttpResponse.json(MOCK_CONSULTATION_TYPES)
  }),

  // GET /api/v1/portal/booking/veterinarians
  http.get(`${PORTAL_BASE}/veterinarians`, async ({ request }) => {
    await delay(150)
    const token = getPortalToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })
    return HttpResponse.json(MOCK_VETERINARIANS)
  }),

  // GET /api/v1/portal/booking/pets
  http.get(`${PORTAL_BASE}/pets`, async ({ request }) => {
    await delay(150)
    const token = getPortalToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })
    return HttpResponse.json(MOCK_BOOKING_PETS)
  }),

  // POST /api/v1/portal/booking/appointments
  http.post(`${PORTAL_BASE}/appointments`, async ({ request }) => {
    await delay(400)
    const token = getPortalToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    const body = await request.json() as CreateBookingAppointmentRequest

    if (!body.petId || !body.consultationTypeId || !body.slotStartsAt) {
      return HttpResponse.json(
        { title: 'petId, consultationTypeId, and slotStartsAt are required.' },
        { status: 422 }
      )
    }

    const newAppointment = {
      id: crypto.randomUUID(),
      ...body,
      createdAt: new Date().toISOString(),
    }
    createdBookingAppointments.push(newAppointment)

    return HttpResponse.json(newAppointment, { status: 201 })
  }),
]
