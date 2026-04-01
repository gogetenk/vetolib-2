import { http, HttpResponse, delay } from 'msw'

// Mock clinic group data — UAE-based multi-clinic group
let MOCK_CLINICS = [
  {
    id: 'clinic-001',
    name: 'Desert Paws Clinic',
    address: 'Al Wasl Road, Jumeirah 2, Dubai',
    totalPatients: 342,
    monthlyRevenue: 48500,
  },
  {
    id: 'clinic-002',
    name: 'Al Barsha Vets',
    address: 'Al Barsha 1, Sheikh Zayed Road, Dubai',
    totalPatients: 218,
    monthlyRevenue: 32100,
  },
  {
    id: 'clinic-003',
    name: 'Marina Pet Care',
    address: 'Dubai Marina Walk, Tower 5, Dubai',
    totalPatients: 156,
    monthlyRevenue: 21400,
  },
]

let MOCK_GROUP_NAME = 'Desert Paws Group'

// Clinics available for search (not yet in group)
const SEARCHABLE_CLINICS = [
  { id: 'clinic-004', name: 'JLT Pet Hospital', address: 'Cluster D, JLT, Dubai' },
  { id: 'clinic-005', name: 'Abu Dhabi Vet Center', address: 'Khalifa City A, Abu Dhabi' },
  { id: 'clinic-006', name: 'Sharjah Animal Clinic', address: 'Al Majaz 3, Sharjah' },
]

function generateToken(payload: object): string {
  const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }))
  const body = btoa(
    JSON.stringify({
      ...payload,
      exp: Math.floor(Date.now() / 1000) + 3600,
      iat: Math.floor(Date.now() / 1000),
    })
  )
  return `${header}.${body}.mock-signature`
}

function parseToken(authHeader: string | null): Record<string, unknown> | null {
  if (!authHeader) return null
  try {
    const token = authHeader.replace('Bearer ', '')
    const base64 = token.split('.')[1]
    return JSON.parse(atob(base64.replace(/-/g, '+').replace(/_/g, '/')))
  } catch {
    return null
  }
}

export const clinicGroupHandlers = [
  // GET /api/v1/clinic-groups/:groupId
  http.get('/api/v1/clinic-groups/:groupId', async ({ request }) => {
    const url = new URL(request.url)
    // Avoid matching /stats and /clinics sub-paths
    if (url.pathname.includes('/stats') || url.pathname.includes('/clinics')) {
      return
    }
    await delay(150)
    return HttpResponse.json({
      id: 'group-001',
      name: MOCK_GROUP_NAME,
      clinics: MOCK_CLINICS,
    })
  }),

  // GET /api/v1/clinic-groups/:groupId/stats
  http.get('/api/v1/clinic-groups/:groupId/stats', async () => {
    await delay(100)
    const totalPatients = MOCK_CLINICS.reduce((sum, c) => sum + c.totalPatients, 0)
    const totalRevenue = MOCK_CLINICS.reduce((sum, c) => sum + c.monthlyRevenue, 0)
    return HttpResponse.json({
      totalPatients,
      totalRevenue,
      clinicCount: MOCK_CLINICS.length,
    })
  }),

  // GET /api/v1/clinic-groups/:groupId/clinics
  http.get('/api/v1/clinic-groups/:groupId/clinics', async () => {
    await delay(150)
    return HttpResponse.json({ clinics: MOCK_CLINICS })
  }),

  // PUT /api/v1/clinic-groups/:groupId
  http.put('/api/v1/clinic-groups/:groupId', async ({ request }) => {
    await delay(150)
    const body = (await request.json()) as { name: string }
    MOCK_GROUP_NAME = body.name
    return new HttpResponse(null, { status: 204 })
  }),

  // POST /api/v1/clinic-groups/:groupId/clinics — add clinic to group
  http.post('/api/v1/clinic-groups/:groupId/clinics', async ({ request }) => {
    await delay(200)
    const body = (await request.json()) as { clinicId: string }
    const found = SEARCHABLE_CLINICS.find((c) => c.id === body.clinicId)
    if (!found) {
      return HttpResponse.json(
        { code: 'CLINIC_NOT_FOUND', title: 'Clinic not found' },
        { status: 404 }
      )
    }
    MOCK_CLINICS = [...MOCK_CLINICS, { ...found, totalPatients: 0, monthlyRevenue: 0 }]
    return new HttpResponse(null, { status: 201 })
  }),

  // DELETE /api/v1/clinic-groups/:groupId/clinics/:clinicId
  http.delete('/api/v1/clinic-groups/:groupId/clinics/:clinicId', async ({ params }) => {
    await delay(150)
    const clinicId = params.clinicId as string
    MOCK_CLINICS = MOCK_CLINICS.filter((c) => c.id !== clinicId)
    return new HttpResponse(null, { status: 204 })
  }),

  // GET /api/v1/clinics/search?q=...
  http.get('/api/v1/clinics/search', async ({ request }) => {
    await delay(150)
    const url = new URL(request.url)
    const query = (url.searchParams.get('q') ?? '').toLowerCase()
    const existingIds = new Set(MOCK_CLINICS.map((c) => c.id))
    const results = SEARCHABLE_CLINICS.filter(
      (c) => !existingIds.has(c.id) && (c.name.toLowerCase().includes(query) || c.address.toLowerCase().includes(query))
    )
    return HttpResponse.json(results)
  }),

  // POST /api/v1/auth/switch-clinic
  http.post('/api/v1/auth/switch-clinic', async ({ request }) => {
    await delay(200)
    const body = (await request.json()) as { clinicId: string }
    const { clinicId } = body

    const targetClinic = MOCK_CLINICS.find((c) => c.id === clinicId)
    if (!targetClinic) {
      return HttpResponse.json(
        { code: 'CLINIC_NOT_FOUND', title: 'Clinic not found' },
        { status: 404 }
      )
    }

    // Parse current user from auth header to preserve identity
    const payload = parseToken(request.headers.get('Authorization'))
    const email = (payload?.sub as string) ?? 'dr.sarah@desertpaws.ae'
    const name = (payload?.name as string) ?? 'Dr. Sarah Johnson'
    const role = (payload?.role as string) ?? 'VET'

    const accessToken = generateToken({
      sub: email,
      clinicId: targetClinic.id,
      clinicName: targetClinic.name,
      clinicGroupId: 'group-001',
      name,
      role,
    })
    const refreshToken = generateToken({ sub: email, type: 'refresh' })

    return HttpResponse.json({
      accessToken,
      refreshToken,
      expiresIn: 3600,
    })
  }),
]
