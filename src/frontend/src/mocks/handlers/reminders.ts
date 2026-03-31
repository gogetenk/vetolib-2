import { http, HttpResponse } from 'msw'
import type { ReminderConfigDto, ReminderLogDto } from '@/lib/api/reminders'

// In-memory store for reminder configuration
let reminderConfig: ReminderConfigDto = {
  appointment24hEnabled: true,
  vaccinationDueEnabled: true,
  followUpEnabled: false,
  appointment24hLeadTimeHours: 24,
  vaccinationDueLeadTimeDays: 30,
  preferredReminderChannel: 'Both',
}

const reminderLogs: ReminderLogDto[] = [
  {
    id: '1',
    sentAt: '2026-03-20T09:30:00Z',
    type: 'appointment',
    patientName: 'Buddy (Golden Retriever)',
    ownerName: 'Ahmed Al Mansouri',
    status: 'sent',
  },
  {
    id: '2',
    sentAt: '2026-03-20T08:15:00Z',
    type: 'vaccination',
    patientName: 'Luna (Persian Cat)',
    ownerName: 'Fatima Al Zahra',
    status: 'sent',
  },
  {
    id: '3',
    sentAt: '2026-03-19T14:00:00Z',
    type: 'follow_up',
    patientName: 'Max (German Shepherd)',
    ownerName: 'Omar Hassan',
    status: 'failed',
    errorMessage: 'Email delivery failed — invalid address',
  },
  {
    id: '4',
    sentAt: '2026-03-19T10:45:00Z',
    type: 'appointment',
    patientName: 'Coco (Cockatiel)',
    ownerName: 'Sarah Al Qasimi',
    status: 'sent',
  },
  {
    id: '5',
    sentAt: '2026-03-18T16:20:00Z',
    type: 'vaccination',
    patientName: 'Rocky (Rottweiler)',
    ownerName: 'Khalid bin Rashid',
    status: 'pending',
  },
  {
    id: '6',
    sentAt: '2026-03-18T11:00:00Z',
    type: 'follow_up',
    patientName: 'Milo (Ragdoll Cat)',
    ownerName: 'Aisha Mohammed',
    status: 'sent',
  },
]

export const reminderHandlers = [
  // GET /api/v1/notifications/reminders/config
  http.get('/api/v1/notifications/reminders/config', () => {
    return HttpResponse.json<ReminderConfigDto>(reminderConfig)
  }),

  // PUT /api/v1/notifications/reminders/config
  http.put('/api/v1/notifications/reminders/config', async ({ request }) => {
    const body = (await request.json()) as ReminderConfigDto
    reminderConfig = { ...body }
    return new HttpResponse(null, { status: 204 })
  }),

  // GET /api/v1/notifications/reminders/logs
  http.get('/api/v1/notifications/reminders/logs', () => {
    return HttpResponse.json<ReminderLogDto[]>(reminderLogs)
  }),
]
