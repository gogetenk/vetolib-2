import { http, HttpResponse } from 'msw'
import type { WorkingHoursDto, WorkingHoursDayDto } from '@/lib/api/working-hours'

// UAE defaults: Sun-Thu 08:00-18:00, Fri 08:00-12:00, Sat closed
const DEFAULT_DAYS: WorkingHoursDayDto[] = [
  {
    dayOfWeek: 'Sunday',
    isOpen: true,
    openTime: '08:00',
    closeTime: '18:00',
    breakStartTime: null,
    breakEndTime: null,
  },
  {
    dayOfWeek: 'Monday',
    isOpen: true,
    openTime: '08:00',
    closeTime: '18:00',
    breakStartTime: null,
    breakEndTime: null,
  },
  {
    dayOfWeek: 'Tuesday',
    isOpen: true,
    openTime: '08:00',
    closeTime: '18:00',
    breakStartTime: null,
    breakEndTime: null,
  },
  {
    dayOfWeek: 'Wednesday',
    isOpen: true,
    openTime: '08:00',
    closeTime: '18:00',
    breakStartTime: null,
    breakEndTime: null,
  },
  {
    dayOfWeek: 'Thursday',
    isOpen: true,
    openTime: '08:00',
    closeTime: '18:00',
    breakStartTime: null,
    breakEndTime: null,
  },
  {
    dayOfWeek: 'Friday',
    isOpen: true,
    openTime: '08:00',
    closeTime: '12:00',
    breakStartTime: null,
    breakEndTime: null,
  },
  {
    dayOfWeek: 'Saturday',
    isOpen: false,
    openTime: null,
    closeTime: null,
    breakStartTime: null,
    breakEndTime: null,
  },
]

// In-memory mutable store
let workingHoursDays: WorkingHoursDayDto[] = structuredClone(DEFAULT_DAYS)

export const workingHoursHandlers = [
  // GET /api/v1/preferences/working-hours
  http.get('/api/v1/preferences/working-hours', () => {
    const response: WorkingHoursDto = { days: workingHoursDays }
    return HttpResponse.json(response)
  }),

  // PUT /api/v1/preferences/working-hours
  http.put('/api/v1/preferences/working-hours', async ({ request }) => {
    const body = (await request.json()) as { days: WorkingHoursDayDto[] }

    // Basic validation
    if (!body.days || body.days.length !== 7) {
      return HttpResponse.json(
        { title: 'Working hours must include all 7 days of the week', status: 400 },
        { status: 400 }
      )
    }

    for (const day of body.days) {
      if (day.isOpen && (!day.openTime || !day.closeTime)) {
        return HttpResponse.json(
          {
            title: `Open/close times are required when ${day.dayOfWeek} is marked as open`,
            status: 400,
          },
          { status: 400 }
        )
      }
    }

    workingHoursDays = structuredClone(body.days)
    return new HttpResponse(null, { status: 204 })
  }),
]
