import { http, HttpResponse, delay } from 'msw'
import type { HealthAlertDto, AppointmentPreFillDto } from '@/lib/api/health-alerts'

const MOCK_ALERTS: HealthAlertDto[] = [
  {
    id: 'ha-0001-0000-0000-000000000001',
    patientId: 'p-0001-0000-0000-000000000001',
    patientName: 'Simba',
    breed: 'Arabian Mau',
    species: 'Cat',
    age: '6 years',
    ownerName: 'Fatima Al-Mansoori',
    title: 'Overdue Rabies Vaccination',
    description: 'Rabies vaccination was due 45 days ago. UAE law requires annual rabies vaccination for all cats.',
    severity: 'High',
    status: 'Active',
    createdAt: new Date(Date.now() - 2 * 86400_000).toISOString(),
  },
  {
    id: 'ha-0001-0000-0000-000000000002',
    patientId: 'p-0001-0000-0000-000000000002',
    patientName: 'Rocky',
    breed: 'Saluki',
    species: 'Dog',
    age: '8 years',
    ownerName: 'Ahmed Al-Rashid',
    title: 'Senior Wellness Screening Due',
    description: 'Rocky is 8 years old and has not had a senior wellness panel in over 14 months. Blood work and cardiac screening recommended.',
    severity: 'High',
    status: 'Active',
    createdAt: new Date(Date.now() - 5 * 86400_000).toISOString(),
  },
  {
    id: 'ha-0001-0000-0000-000000000003',
    patientId: 'p-0001-0000-0000-000000000003',
    patientName: 'Nala',
    breed: 'Persian',
    species: 'Cat',
    age: '4 years',
    ownerName: 'Mariam Hussain',
    title: 'Dental Check Recommended',
    description: 'Last dental examination was 18 months ago. Persians are prone to dental disease — a check-up is recommended.',
    severity: 'Medium',
    status: 'Active',
    createdAt: new Date(Date.now() - 3 * 86400_000).toISOString(),
  },
  {
    id: 'ha-0001-0000-0000-000000000004',
    patientId: 'p-0001-0000-0000-000000000004',
    patientName: 'Max',
    breed: 'German Shepherd',
    species: 'Dog',
    age: '5 years',
    ownerName: 'Khalid bin Saeed',
    title: 'Weight Trend Increasing',
    description: 'Max has gained 3.2 kg over the past 4 months. Current weight 42 kg is above ideal range for breed and age.',
    severity: 'Medium',
    status: 'Active',
    createdAt: new Date(Date.now() - 7 * 86400_000).toISOString(),
  },
  {
    id: 'ha-0001-0000-0000-000000000005',
    patientId: 'p-0001-0000-0000-000000000005',
    patientName: 'Cleo',
    breed: 'Siamese',
    species: 'Cat',
    age: '2 years',
    ownerName: 'Sarah Johnson',
    title: 'Parasite Prevention Renewal',
    description: 'Flea and tick prevention subscription expired 10 days ago. Renewal recommended for continuous protection.',
    severity: 'Low',
    status: 'Active',
    createdAt: new Date(Date.now() - 1 * 86400_000).toISOString(),
  },
  {
    id: 'ha-0001-0000-0000-000000000006',
    patientId: 'p-0001-0000-0000-000000000001',
    patientName: 'Simba',
    breed: 'Arabian Mau',
    species: 'Cat',
    age: '6 years',
    ownerName: 'Fatima Al-Mansoori',
    title: 'Annual Deworming Due',
    description: 'Last deworming treatment was administered 13 months ago. Annual deworming is recommended.',
    severity: 'Low',
    status: 'Dismissed',
    dismissReason: 'Owner confirmed deworming done at another clinic.',
    createdAt: new Date(Date.now() - 30 * 86400_000).toISOString(),
  },
]

export const healthAlertHandlers = [
  // GET /api/v1/ai/health-alerts — all active alerts
  http.get('/api/v1/ai/health-alerts', async () => {
    await delay(300)
    const activeAlerts = MOCK_ALERTS.filter(a => a.status !== 'Dismissed')
    return HttpResponse.json(activeAlerts)
  }),

  // GET /api/v1/ai/health-alerts/patient/:patientId — alerts for a specific patient
  http.get('/api/v1/ai/health-alerts/patient/:patientId', async ({ params }) => {
    await delay(200)
    const patientAlerts = MOCK_ALERTS.filter(a => a.patientId === params.patientId)
    return HttpResponse.json(patientAlerts)
  }),

  // PATCH /api/v1/ai/health-alerts/:id/dismiss
  http.patch('/api/v1/ai/health-alerts/:id/dismiss', async ({ params, request }) => {
    await delay(200)
    const body = await request.json() as { reason: string }
    const alert = MOCK_ALERTS.find(a => a.id === params.id)
    if (!alert) return new HttpResponse(null, { status: 404 })
    alert.status = 'Dismissed'
    alert.dismissReason = body.reason
    return new HttpResponse(null, { status: 200 })
  }),

  // PATCH /api/v1/ai/health-alerts/:id/acknowledge
  http.patch('/api/v1/ai/health-alerts/:id/acknowledge', async ({ params }) => {
    await delay(200)
    const alert = MOCK_ALERTS.find(a => a.id === params.id)
    if (!alert) return new HttpResponse(null, { status: 404 })
    alert.status = 'Acknowledged'
    return new HttpResponse(null, { status: 200 })
  }),

  // POST /api/v1/ai/health-alerts/:id/convert-to-appointment
  http.post('/api/v1/ai/health-alerts/:id/convert-to-appointment', async ({ params }) => {
    await delay(300)
    const alert = MOCK_ALERTS.find(a => a.id === params.id)
    if (!alert) return new HttpResponse(null, { status: 404 })

    const preFill: AppointmentPreFillDto = {
      patientId: alert.patientId,
      patientName: alert.patientName,
      ownerName: alert.ownerName,
      reason: alert.title,
      suggestedDate: new Date(Date.now() + 3 * 86400_000).toISOString(),
      suggestedDurationMinutes: 30,
    }
    return HttpResponse.json(preFill)
  }),
]
