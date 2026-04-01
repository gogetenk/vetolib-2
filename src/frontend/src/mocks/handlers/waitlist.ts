import { http, HttpResponse } from 'msw'
import type { WaitlistEntryDto, CreateWaitlistEntryRequest } from '@/lib/api/waitlist'

let nextId = 6

const waitlistEntries: WaitlistEntryDto[] = [
  {
    id: '1',
    patientId: 'p1',
    patientName: 'Buddy (Golden Retriever)',
    ownerName: 'Ahmed Al Mansouri',
    reason: 'Dental cleaning — waiting for cancellation slot',
    preferredDay: 'Sunday',
    preferredTime: 'Morning',
    priority: 'NORMAL',
    createdAt: '2026-03-28T09:00:00Z',
  },
  {
    id: '2',
    patientId: 'p2',
    patientName: 'Luna (Persian Cat)',
    ownerName: 'Fatima Al Zahra',
    reason: 'Follow-up ultrasound',
    preferredDay: 'Monday',
    preferredTime: 'Afternoon',
    priority: 'HIGH',
    createdAt: '2026-03-27T14:30:00Z',
  },
  {
    id: '3',
    patientId: 'p3',
    patientName: 'Max (German Shepherd)',
    ownerName: 'Omar Hassan',
    reason: 'Vaccination booster — overdue',
    preferredDay: 'Any',
    preferredTime: 'Any',
    priority: 'URGENT',
    createdAt: '2026-03-26T11:15:00Z',
  },
  {
    id: '4',
    patientId: 'p4',
    patientName: 'Coco (Cockatiel)',
    ownerName: 'Sarah Al Qasimi',
    reason: 'Wing trim appointment',
    preferredDay: 'Wednesday',
    preferredTime: 'Morning',
    priority: 'LOW',
    createdAt: '2026-03-25T16:45:00Z',
  },
  {
    id: '5',
    patientId: 'p5',
    patientName: 'Rocky (Rottweiler)',
    ownerName: 'Khalid bin Rashid',
    reason: 'Knee surgery consultation',
    preferredDay: 'Tuesday',
    preferredTime: 'Afternoon',
    priority: 'HIGH',
    createdAt: '2026-03-24T08:20:00Z',
  },
]

export const waitlistHandlers = [
  // GET /api/v1/waitlist
  http.get('/api/v1/waitlist', () => {
    return HttpResponse.json<WaitlistEntryDto[]>(waitlistEntries)
  }),

  // POST /api/v1/waitlist
  http.post('/api/v1/waitlist', async ({ request }) => {
    const body = (await request.json()) as CreateWaitlistEntryRequest
    const newEntry: WaitlistEntryDto = {
      id: String(nextId++),
      patientId: body.patientId,
      patientName: 'New Patient',
      ownerName: 'Owner',
      reason: body.reason,
      preferredDay: body.preferredDay,
      preferredTime: body.preferredTime,
      priority: body.priority,
      createdAt: new Date().toISOString(),
    }
    waitlistEntries.push(newEntry)
    return HttpResponse.json<WaitlistEntryDto>(newEntry, { status: 201 })
  }),

  // DELETE /api/v1/waitlist/:id
  http.delete('/api/v1/waitlist/:id', ({ params }) => {
    const { id } = params
    const index = waitlistEntries.findIndex((e) => e.id === id)
    if (index === -1) {
      return new HttpResponse(null, { status: 404 })
    }
    waitlistEntries.splice(index, 1)
    return new HttpResponse(null, { status: 204 })
  }),
]
