import { http, HttpResponse, delay } from 'msw'

export interface UserDto {
  id: string
  email: string
  fullName: string
  role: 'ADMIN' | 'VET' | 'ASSISTANT' | 'RECEPTIONIST'
  isActive: boolean
}

export interface InviteUserRequest {
  email: string
  fullName: string
  role: 'VET' | 'ASSISTANT' | 'RECEPTIONIST'
}

export interface ChangeRoleRequest {
  role: 'VET' | 'ASSISTANT' | 'RECEPTIONIST'
}

const MOCK_USERS: UserDto[] = [
  {
    id: 'u-001',
    email: 'dr.sarah@desertpaws.ae',
    fullName: 'Dr. Sarah Johnson',
    role: 'VET',
    isActive: true,
  },
  {
    id: 'u-002',
    email: 'reception@desertpaws.ae',
    fullName: 'Amira Hassan',
    role: 'RECEPTIONIST',
    isActive: true,
  },
  {
    id: 'u-003',
    email: 'admin@desertpaws.ae',
    fullName: 'Omar Al-Rashid',
    role: 'ADMIN',
    isActive: true,
  },
]

export const userHandlers = [
  // GET /api/users
  http.get('/api/users', async () => {
    await delay(150)
    return HttpResponse.json(MOCK_USERS.filter(u => u.isActive !== undefined))
  }),

  // POST /api/users — invite a new member
  http.post('/api/users', async ({ request }) => {
    await delay(200)
    const body = await request.json() as InviteUserRequest

    if (!body.email || !body.fullName || !body.role) {
      return HttpResponse.json(
        { title: 'Email, full name and role are required' },
        { status: 400 }
      )
    }

    const existing = MOCK_USERS.find(u => u.email === body.email)
    if (existing) {
      return HttpResponse.json(
        { title: 'A user with this email already exists' },
        { status: 409 }
      )
    }

    const newUser: UserDto = {
      id: `u-${Date.now()}`,
      email: body.email,
      fullName: body.fullName,
      role: body.role,
      isActive: true,
    }
    MOCK_USERS.push(newUser)

    // Return the new user along with a temporary password
    return HttpResponse.json(
      {
        user: newUser,
        temporaryPassword: 'Temp@' + Math.random().toString(36).slice(2, 10),
      },
      { status: 201 }
    )
  }),

  // PATCH /api/users/:id/role — change user role
  http.patch('/api/users/:id/role', async ({ params, request }) => {
    await delay(150)
    const body = await request.json() as ChangeRoleRequest
    const user = MOCK_USERS.find(u => u.id === params.id)

    if (!user) {
      return HttpResponse.json({ title: 'User not found' }, { status: 404 })
    }

    if (!body.role) {
      return HttpResponse.json({ title: 'Role is required' }, { status: 400 })
    }

    user.role = body.role
    return HttpResponse.json(user)
  }),

  // DELETE /api/users/:id — deactivate user
  http.delete('/api/users/:id', async ({ params }) => {
    await delay(150)
    const user = MOCK_USERS.find(u => u.id === params.id)

    if (!user) {
      return HttpResponse.json({ title: 'User not found' }, { status: 404 })
    }

    user.isActive = false
    return new HttpResponse(null, { status: 204 })
  }),
]
