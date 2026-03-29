import { http, HttpResponse } from 'msw'
import type { WeightEntryDto, WeightCurvePointDto, CreateWeightRequest } from '@/lib/api/weights'

// Realistic Arabian Horse weight data (420-460kg range) for patient pat-0000-0000-0000-000000000001
// Also includes data for smaller animals
const MOCK_WEIGHTS: Record<string, WeightEntryDto[]> = {
  'pat-0000-0000-0000-000000000001': [
    { id: 'w-001', patientId: 'pat-0000-0000-0000-000000000001', weightKg: 32.5, recordedAt: '2025-11-20T09:00:00.000Z', note: 'Annual check-up', recordedBy: 'Dr. Sarah Johnson' },
    { id: 'w-002', patientId: 'pat-0000-0000-0000-000000000001', weightKg: 33.0, recordedAt: '2025-06-10T10:30:00.000Z', note: 'Limping visit — weight stable', recordedBy: 'Dr. Ahmed Khalil' },
    { id: 'w-003', patientId: 'pat-0000-0000-0000-000000000001', weightKg: 32.0, recordedAt: '2024-12-05T11:00:00.000Z', note: 'Skin irritation visit', recordedBy: 'Dr. Sarah Johnson' },
    { id: 'w-004', patientId: 'pat-0000-0000-0000-000000000001', weightKg: 31.2, recordedAt: '2024-06-15T08:30:00.000Z', note: null, recordedBy: 'Dr. Ahmed Khalil' },
    { id: 'w-005', patientId: 'pat-0000-0000-0000-000000000001', weightKg: 30.0, recordedAt: '2024-01-10T14:00:00.000Z', note: 'Post-holiday check', recordedBy: 'Dr. Sarah Johnson' },
    { id: 'w-006', patientId: 'pat-0000-0000-0000-000000000001', weightKg: 28.5, recordedAt: '2023-07-20T09:00:00.000Z', note: 'Summer checkup', recordedBy: 'Dr. Ahmed Khalil' },
    { id: 'w-007', patientId: 'pat-0000-0000-0000-000000000001', weightKg: 26.0, recordedAt: '2023-01-15T10:00:00.000Z', note: 'Growing well', recordedBy: 'Dr. Sarah Johnson' },
    { id: 'w-008', patientId: 'pat-0000-0000-0000-000000000001', weightKg: 22.5, recordedAt: '2022-06-01T11:00:00.000Z', note: null, recordedBy: 'Dr. Ahmed Khalil' },
    { id: 'w-009', patientId: 'pat-0000-0000-0000-000000000001', weightKg: 18.0, recordedAt: '2021-12-10T09:30:00.000Z', note: 'Puppy vaccination', recordedBy: 'Dr. Sarah Johnson' },
    { id: 'w-010', patientId: 'pat-0000-0000-0000-000000000001', weightKg: 12.0, recordedAt: '2021-06-05T14:00:00.000Z', note: 'First visit', recordedBy: 'Dr. Ahmed Khalil' },
    { id: 'w-011', patientId: 'pat-0000-0000-0000-000000000001', weightKg: 8.5, recordedAt: '2020-12-20T10:00:00.000Z', note: 'Initial weigh-in', recordedBy: 'Dr. Sarah Johnson' },
  ],
  'pat-0000-0000-0000-000000000002': [
    { id: 'w-020', patientId: 'pat-0000-0000-0000-000000000002', weightKg: 3.8, recordedAt: '2026-02-10T14:00:00.000Z', note: 'Follow-up visit', recordedBy: 'Dr. Ahmed Khalil' },
    { id: 'w-021', patientId: 'pat-0000-0000-0000-000000000002', weightKg: 3.5, recordedAt: '2025-09-01T10:00:00.000Z', note: 'Vaccination day', recordedBy: 'Dr. Sarah Johnson' },
    { id: 'w-022', patientId: 'pat-0000-0000-0000-000000000002', weightKg: 3.2, recordedAt: '2025-03-15T09:00:00.000Z', note: null, recordedBy: 'Dr. Ahmed Khalil' },
  ],
  'pat-0000-0000-0000-000000000005': [
    { id: 'w-050', patientId: 'pat-0000-0000-0000-000000000005', weightKg: 520.0, recordedAt: '2026-01-10T08:00:00.000Z', note: 'Routine check', recordedBy: 'Dr. Sarah Johnson' },
    { id: 'w-051', patientId: 'pat-0000-0000-0000-000000000005', weightKg: 515.0, recordedAt: '2025-07-20T09:00:00.000Z', note: 'Summer weight', recordedBy: 'Dr. Ahmed Khalil' },
    { id: 'w-052', patientId: 'pat-0000-0000-0000-000000000005', weightKg: 510.0, recordedAt: '2025-01-15T10:00:00.000Z', note: null, recordedBy: 'Dr. Sarah Johnson' },
    { id: 'w-053', patientId: 'pat-0000-0000-0000-000000000005', weightKg: 450.0, recordedAt: '2024-06-01T08:30:00.000Z', note: 'Growth phase', recordedBy: 'Dr. Ahmed Khalil' },
    { id: 'w-054', patientId: 'pat-0000-0000-0000-000000000005', weightKg: 440.0, recordedAt: '2024-01-10T09:00:00.000Z', note: 'Arabian Horse — healthy', recordedBy: 'Dr. Sarah Johnson' },
    { id: 'w-055', patientId: 'pat-0000-0000-0000-000000000005', weightKg: 430.0, recordedAt: '2023-07-15T10:00:00.000Z', note: null, recordedBy: 'Dr. Ahmed Khalil' },
    { id: 'w-056', patientId: 'pat-0000-0000-0000-000000000005', weightKg: 420.0, recordedAt: '2023-01-20T11:00:00.000Z', note: 'Starting weight tracking', recordedBy: 'Dr. Sarah Johnson' },
  ],
}

export const weightHandlers = [
  // GET /api/patients/:id/weights — full history, most recent first
  http.get('/api/patients/:id/weights', ({ params }) => {
    const patientId = params.id as string
    const weights = MOCK_WEIGHTS[patientId] ?? []
    // Return sorted most recent first
    const sorted = [...weights].sort(
      (a, b) => new Date(b.recordedAt).getTime() - new Date(a.recordedAt).getTime()
    )
    return HttpResponse.json(sorted)
  }),

  // GET /api/patients/:id/weights/curve — date-sorted for charting
  http.get('/api/patients/:id/weights/curve', ({ params }) => {
    const patientId = params.id as string
    const weights = MOCK_WEIGHTS[patientId] ?? []
    // Return sorted oldest first for line chart
    const sorted = [...weights].sort(
      (a, b) => new Date(a.recordedAt).getTime() - new Date(b.recordedAt).getTime()
    )
    const curve: WeightCurvePointDto[] = sorted.map(w => ({
      date: w.recordedAt.split('T')[0],
      weightKg: w.weightKg,
    }))
    return HttpResponse.json(curve)
  }),

  // POST /api/patients/:id/weights — add a new weight entry
  http.post('/api/patients/:id/weights', async ({ params, request }) => {
    const patientId = params.id as string
    const body = (await request.json()) as CreateWeightRequest

    // Validation
    if (!body.weightKg || body.weightKg <= 0 || body.weightKg > 10000) {
      return HttpResponse.json(
        { title: 'Weight must be between 0 and 10,000 kg' },
        { status: 400 }
      )
    }

    const newEntry: WeightEntryDto = {
      id: `w-${crypto.randomUUID().slice(0, 8)}`,
      patientId,
      weightKg: body.weightKg,
      recordedAt: body.recordedAt || new Date().toISOString(),
      note: body.note ?? null,
      recordedBy: 'Dr. Sarah Johnson', // simulated current user
    }

    if (!MOCK_WEIGHTS[patientId]) {
      MOCK_WEIGHTS[patientId] = []
    }
    MOCK_WEIGHTS[patientId].unshift(newEntry)

    return HttpResponse.json(newEntry, { status: 201 })
  }),
]
