import { http, HttpResponse, delay } from 'msw'

interface LoginRequest {
  email: string
  password: string
}

interface RefreshRequest {
  refreshToken: string
}

// Track failed attempts per email for account lockout simulation
const failedAttempts: Record<string, number> = {}
const lockedAccounts: Set<string> = new Set()

// Mock users: UAE clinic staff
const MOCK_USERS: Record<string, { password: string; clinicId: string; clinicName: string; clinicGroupId: string; name: string; role: string }> = {
  'dr.sarah@desertpaws.ae': {
    password: 'Secure123!',
    clinicId: 'clinic-001',
    clinicName: 'Desert Paws Clinic',
    clinicGroupId: 'group-001',
    name: 'Dr. Sarah Johnson',
    role: 'VET',
  },
  'dr.omar@albarsha.ae': {
    password: 'Secure456!',
    clinicId: 'clinic-002',
    clinicName: 'Al Barsha Vets',
    clinicGroupId: 'group-001',
    name: 'Dr. Omar Al-Rashid',
    role: 'VET',
  },
  'assistant@desertpaws.ae': {
    password: 'Secure123!',
    clinicId: 'clinic-001',
    clinicName: 'Desert Paws Clinic',
    clinicGroupId: 'group-001',
    name: 'Mariam Al-Zaabi',
    role: 'ASSISTANT',
  },
  'reception@desertpaws.ae': {
    password: 'Secure123!',
    clinicId: 'clinic-001',
    clinicName: 'Desert Paws Clinic',
    clinicGroupId: 'group-001',
    name: 'Khalid Al-Nuaimi',
    role: 'RECEPTIONIST',
  },
}

function generateToken(payload: object): string {
  // Simple base64-encoded fake JWT for MSW purposes
  const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }))
  const body = btoa(JSON.stringify({ ...payload, exp: Math.floor(Date.now() / 1000) + 3600, iat: Math.floor(Date.now() / 1000) }))
  return `${header}.${body}.mock-signature`
}

interface RegisterClinicRequest {
  clinicName: string
  email: string
  password: string
  phone: string
}

// Track registered emails to simulate persistence within the session
const registeredEmails: Set<string> = new Set(['taken@test.ae'])

export const authHandlers = [
  // POST /api/v1/auth/register-clinic
  http.post('/api/v1/auth/register-clinic', async ({ request }) => {
    await delay(300)
    const body = await request.json() as RegisterClinicRequest
    const { email } = body

    if (registeredEmails.has(email)) {
      return HttpResponse.json(
        { code: 'EMAIL_TAKEN', title: 'Email already in use' },
        { status: 409 }
      )
    }

    registeredEmails.add(email)

    // Add user to MOCK_USERS so login works after signup
    MOCK_USERS[email] = {
      password: body.password,
      clinicId: `clinic-${Date.now()}`,
      clinicName: body.clinicName,
      clinicGroupId: `group-${Date.now()}`,
      name: body.clinicName,
      role: 'ADMIN',
    }

    return new HttpResponse(null, { status: 201 })
  }),

  // POST /api/v1/auth/login
  http.post('/api/v1/auth/login', async ({ request }) => {
    await delay(200) // Realistic network delay
    const body = await request.json() as LoginRequest
    const { email, password } = body

    // Check if account is locked
    if (lockedAccounts.has(email)) {
      return HttpResponse.json(
        { code: 'ACCOUNT_LOCKED', title: 'Account locked. Try again in 15 minutes.' },
        { status: 423 }
      )
    }

    const user = MOCK_USERS[email]

    if (!user || user.password !== password) {
      // Increment failed attempts
      failedAttempts[email] = (failedAttempts[email] ?? 0) + 1

      if (failedAttempts[email] >= 5) {
        lockedAccounts.add(email)
        return HttpResponse.json(
          { code: 'ACCOUNT_LOCKED', title: 'Account locked. Try again in 15 minutes.' },
          { status: 423 }
        )
      }

      return HttpResponse.json(
        { code: 'INVALID_CREDENTIALS', title: 'Invalid email or password' },
        { status: 401 }
      )
    }

    // Reset failed attempts on success
    failedAttempts[email] = 0

    const accessToken = generateToken({ sub: email, clinicId: user.clinicId, clinicName: user.clinicName, clinicGroupId: user.clinicGroupId, name: user.name, role: user.role })
    const refreshToken = generateToken({ sub: email, type: 'refresh' })

    return HttpResponse.json({
      accessToken,
      refreshToken,
      expiresIn: 3600,
      user: {
        email,
        name: user.name,
        clinicId: user.clinicId,
        clinicName: user.clinicName,
        clinicGroupId: user.clinicGroupId,
        role: user.role,
      },
    })
  }),

  // POST /api/v1/auth/refresh
  http.post('/api/v1/auth/refresh', async ({ request }) => {
    await delay(100)
    const body = await request.json() as RefreshRequest
    const { refreshToken } = body

    if (!refreshToken) {
      return HttpResponse.json(
        { code: 'INVALID_TOKEN', title: 'Invalid refresh token' },
        { status: 401 }
      )
    }

    // Decode the fake token to get the subject
    try {
      const parts = refreshToken.split('.')
      const payload = JSON.parse(atob(parts[1])) as Record<string, unknown>
      const email = payload.sub as string
      const user = MOCK_USERS[email]

      if (!user) {
        return HttpResponse.json(
          { code: 'INVALID_TOKEN', title: 'Invalid refresh token' },
          { status: 401 }
        )
      }

      const newAccessToken = generateToken({ sub: email, clinicId: user.clinicId, clinicName: user.clinicName, clinicGroupId: user.clinicGroupId, name: user.name, role: user.role })
      const newRefreshToken = generateToken({ sub: email, type: 'refresh' })

      return HttpResponse.json({
        accessToken: newAccessToken,
        refreshToken: newRefreshToken,
        expiresIn: 3600,
      })
    } catch {
      return HttpResponse.json(
        { code: 'INVALID_TOKEN', title: 'Invalid refresh token' },
        { status: 401 }
      )
    }
  }),
]
