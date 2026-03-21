import { http, HttpResponse, delay } from 'msw'

// Mock clinic group data — UAE-based multi-clinic group
const MOCK_CLINICS = [
  {
    id: 'clinic-001',
    name: 'Desert Paws Clinic',
    address: 'Al Wasl Road, Jumeirah 2, Dubai',
  },
  {
    id: 'clinic-002',
    name: 'Al Barsha Vets',
    address: 'Al Barsha 1, Sheikh Zayed Road, Dubai',
  },
  {
    id: 'clinic-003',
    name: 'Marina Pet Care',
    address: 'Dubai Marina Walk, Tower 5, Dubai',
  },
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
  // GET /api/v1/clinic-groups/:groupId/clinics
  http.get('/api/v1/clinic-groups/:groupId/clinics', async () => {
    await delay(150)
    return HttpResponse.json({ clinics: MOCK_CLINICS })
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
