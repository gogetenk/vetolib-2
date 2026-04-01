import { http, HttpResponse, delay } from 'msw'

/**
 * MSW handler for GET /api/v1/auth/my-organizations.
 * Returns the Keycloak organizations (clinics) the current user belongs to.
 * Uses the same mock clinic data as the auth handlers for consistency.
 */
export const organizationHandlers = [
  http.get('/api/v1/auth/my-organizations', async ({ request }) => {
    await delay(150)

    // Check for auth header
    const authHeader = request.headers.get('Authorization')
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return HttpResponse.json(
        { code: 'UNAUTHORIZED', title: 'Authentication required' },
        { status: 401 }
      )
    }

    // Decode the fake JWT to determine which user is calling
    try {
      const token = authHeader.replace('Bearer ', '')
      const parts = token.split('.')
      const payload = JSON.parse(atob(parts[1])) as Record<string, unknown>
      const clinicGroupId = payload.clinic_group_id as string | undefined

      // Users in group-001 see both clinics
      if (clinicGroupId === 'group-001') {
        return HttpResponse.json({
          organizations: [
            {
              id: 'clinic-001',
              name: 'Desert Paws Clinic',
              address: 'Al Wasl Road, Jumeirah 1, Dubai',
            },
            {
              id: 'clinic-002',
              name: 'Al Barsha Vets',
              address: 'Al Barsha 1, Dubai',
            },
          ],
        })
      }

      // Default: user belongs to a single organization
      const clinicId = payload.clinic_id as string | undefined
      const clinicName = payload.clinic_name as string | undefined

      if (clinicId && clinicName) {
        return HttpResponse.json({
          organizations: [
            {
              id: clinicId,
              name: clinicName,
              address: 'Dubai, UAE',
            },
          ],
        })
      }

      // No organization data found
      return HttpResponse.json({ organizations: [] })
    } catch {
      return HttpResponse.json(
        { code: 'INVALID_TOKEN', title: 'Invalid token' },
        { status: 401 }
      )
    }
  }),
]
