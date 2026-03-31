import { http, HttpResponse, delay } from 'msw'

// ─── Types ────────────────────────────────────────────────────────────────────

interface ShareLinkDto {
  id: string
  animalId: string
  token: string
  shareUrl: string
  expiresAt: string
  createdAt: string
  accessCount: number
  isRevoked: boolean
}

interface SharedRecordDto {
  animalName: string
  species: string
  breed: string
  ageYears: number
  clinicName: string
  lastVisit: string
  vaccinations: Array<{
    name: string
    date: string
    nextDue: string | null
  }>
  recentConsultations: Array<{
    date: string
    reason: string
    veterinarian: string
    notes: string
  }>
}

// ─── In-memory store ──────────────────────────────────────────────────────────

const shareLinks: ShareLinkDto[] = [
  {
    id: 'share-001',
    animalId: 'pat-0000-0000-0000-000000000001',
    token: 'abc123def456',
    shareUrl: 'http://localhost:3000/en/shared/abc123def456',
    expiresAt: new Date(Date.now() + 48 * 60 * 60 * 1000).toISOString(),
    createdAt: new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString(),
    accessCount: 3,
    isRevoked: false,
  },
]

const PORTAL_BASE = '/api/v1/portal'
const PUBLIC_BASE = '/api/v1/shared'

function getOwnerToken(request: Request): string | null {
  const auth = request.headers.get('Authorization')
  if (!auth) return null
  const parts = auth.split(' ')
  return parts[1] ?? null
}

export const recordSharingHandlers = [
  // POST /api/v1/portal/animals/:id/share — create a share link
  http.post(`${PORTAL_BASE}/animals/:id/share`, async ({ params, request }) => {
    await delay(200)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    const animalId = params.id as string
    const newToken = crypto.randomUUID().replace(/-/g, '').slice(0, 12)
    const shareLink: ShareLinkDto = {
      id: crypto.randomUUID(),
      animalId,
      token: newToken,
      shareUrl: `${typeof window !== 'undefined' ? window.location.origin : 'http://localhost:3000'}/en/shared/${newToken}`,
      expiresAt: new Date(Date.now() + 72 * 60 * 60 * 1000).toISOString(),
      createdAt: new Date().toISOString(),
      accessCount: 0,
      isRevoked: false,
    }
    shareLinks.push(shareLink)

    return HttpResponse.json(shareLink, { status: 201 })
  }),

  // GET /api/v1/portal/shares — list active share links
  http.get(`${PORTAL_BASE}/shares`, async ({ request }) => {
    await delay(150)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    const activeLinks = shareLinks.filter(l => !l.isRevoked)
    return HttpResponse.json(activeLinks)
  }),

  // DELETE /api/v1/portal/shares/:id — revoke a share link
  http.delete(`${PORTAL_BASE}/shares/:id`, async ({ params, request }) => {
    await delay(150)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    const link = shareLinks.find(l => l.id === params.id)
    if (!link) return new HttpResponse(null, { status: 404 })

    link.isRevoked = true
    return new HttpResponse(null, { status: 204 })
  }),

  // GET /api/v1/shared/:token — public shared record view
  http.get(`${PUBLIC_BASE}/:token`, async ({ params }) => {
    await delay(200)
    const tokenParam = params.token as string

    const link = shareLinks.find(l => l.token === tokenParam && !l.isRevoked)
    if (!link) {
      return HttpResponse.json(
        { title: 'This share link is invalid or has expired.' },
        { status: 404 }
      )
    }

    // Check expiry
    if (new Date(link.expiresAt) < new Date()) {
      return HttpResponse.json(
        { title: 'This share link has expired.' },
        { status: 410 }
      )
    }

    link.accessCount += 1

    const record: SharedRecordDto = {
      animalName: 'Max',
      species: 'Dog',
      breed: 'Golden Retriever',
      ageYears: 6,
      clinicName: 'Desert Paws Veterinary Clinic',
      lastVisit: new Date('2026-03-15T09:00:00+04:00').toISOString(),
      vaccinations: [
        {
          name: 'Rabies',
          date: '2025-12-01',
          nextDue: '2026-12-01',
        },
        {
          name: 'DHPP',
          date: '2025-11-15',
          nextDue: '2026-11-15',
        },
        {
          name: 'Bordetella',
          date: '2026-01-10',
          nextDue: null,
        },
      ],
      recentConsultations: [
        {
          date: '2026-03-15',
          reason: 'Annual wellness check',
          veterinarian: 'Dr. Fatima Al Hashemi',
          notes: 'All vitals normal. Weight stable at 32 kg. Recommended dental cleaning.',
        },
        {
          date: '2026-02-01',
          reason: 'Skin irritation on right paw',
          veterinarian: 'Dr. Ahmed Khalil',
          notes: 'Mild allergic dermatitis. Prescribed topical cream for 2 weeks. Follow-up if no improvement.',
        },
      ],
    }

    return HttpResponse.json(record)
  }),
]
