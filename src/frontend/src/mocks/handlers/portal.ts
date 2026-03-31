import { http, HttpResponse, delay } from 'msw'
import {
  MOCK_PORTAL_CONVERSATIONS,
  MOCK_PORTAL_MESSAGES,
  MOCK_PORTAL_PETS,
} from '@/mocks/data/messaging'
import {
  MOCK_PORTAL_ANIMALS,
  MOCK_PORTAL_MEDICAL_RECORDS,
  MOCK_PORTAL_VACCINATIONS,
  MOCK_PORTAL_PRESCRIPTIONS,
  MOCK_PORTAL_WEIGHT_HISTORY,
} from '@/mocks/data/portal-medical'
import type {
  PortalConversationDto,
  PortalMessageDto,
  MessageCategory,
} from '@/lib/api/messaging-types'

// Mutable copies so mutations persist within the session
const portalConversations: PortalConversationDto[] = MOCK_PORTAL_CONVERSATIONS.map(c => ({ ...c }))
const portalMessages: Record<string, PortalMessageDto[]> = Object.fromEntries(
  Object.entries(MOCK_PORTAL_MESSAGES).map(([k, v]) => [k, v.map(m => ({ ...m }))])
)

// Track consent per magic link token (simulated)
const consentGiven = new Set<string>(['valid-magic-token-001'])

// Track daily message counts per owner token
const dailyMessageCount: Record<string, number> = {}
const MAX_MESSAGES_PER_DAY = 5

const BASE = '/api/v1/portal'

function getOwnerToken(request: Request): string | null {
  const auth = request.headers.get('Authorization')
  if (!auth) return null
  // Format: "MagicLink {token}" or "Bearer {token}"
  const parts = auth.split(' ')
  return parts[1] ?? null
}

export const portalHandlers = [
  // GET /api/v1/portal/conversations
  http.get(`${BASE}/conversations`, async ({ request }) => {
    await delay(150)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })
    if (token === 'expired-magic-token') {
      return HttpResponse.json(
        { title: 'This link has expired. Please contact your clinic to receive a new one.' },
        { status: 401 }
      )
    }

    return HttpResponse.json(portalConversations)
  }),

  // GET /api/v1/portal/conversations/:id
  http.get(`${BASE}/conversations/:id`, async ({ params, request }) => {
    await delay(100)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    const conv = portalConversations.find(c => c.id === params.id)
    if (!conv) return new HttpResponse(null, { status: 404 })

    const messages = portalMessages[conv.id] ?? []
    // Internal notes are never included in the portal view
    return HttpResponse.json({ ...conv, messages })
  }),

  // POST /api/v1/portal/conversations
  http.post(`${BASE}/conversations`, async ({ request }) => {
    await delay(250)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    if (!consentGiven.has(token)) {
      return HttpResponse.json(
        { title: 'You must accept the messaging terms and conditions before sending a message.' },
        { status: 403 }
      )
    }

    const count = dailyMessageCount[token] ?? 0
    if (count >= MAX_MESSAGES_PER_DAY) {
      return HttpResponse.json(
        { title: 'You have reached the daily message limit. Please try again tomorrow.' },
        { status: 429 }
      )
    }

    const body = await request.json() as {
      petId: string | null
      subject: string
      category: MessageCategory
      body: string
    }

    if (body.body.length > 2000) {
      return HttpResponse.json(
        { title: 'Message body exceeds 2000 characters.' },
        { status: 422 }
      )
    }

    const newConv: PortalConversationDto = {
      id: crypto.randomUUID(),
      subject: body.subject,
      category: body.category,
      status: 'Open',
      lastMessageAt: new Date().toISOString(),
      createdAt: new Date().toISOString(),
      unreadByOwnerCount: 0,
    }
    portalConversations.unshift(newConv)

    const firstMessage: PortalMessageDto = {
      id: crypto.randomUUID(),
      sender: 'Owner',
      senderName: 'You',
      body: body.body,
      sentAt: new Date().toISOString(),
      attachments: [],
    }
    portalMessages[newConv.id] = [firstMessage]

    dailyMessageCount[token] = count + 1

    return HttpResponse.json(newConv, { status: 201 })
  }),

  // POST /api/v1/portal/conversations/:id/messages
  http.post(`${BASE}/conversations/:id/messages`, async ({ params, request }) => {
    await delay(200)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    const conv = portalConversations.find(c => c.id === params.id)
    if (!conv) return new HttpResponse(null, { status: 404 })

    if (conv.status === 'Closed') {
      return HttpResponse.json(
        { title: 'This conversation is closed. Please start a new conversation.' },
        { status: 422 }
      )
    }

    const count = dailyMessageCount[token] ?? 0
    if (count >= MAX_MESSAGES_PER_DAY) {
      return HttpResponse.json(
        { title: 'You have reached the daily message limit. Please try again tomorrow.' },
        { status: 429 }
      )
    }

    const body = await request.json() as { body: string }

    if (body.body.length > 2000) {
      return HttpResponse.json(
        { title: 'Message body exceeds 2000 characters.' },
        { status: 422 }
      )
    }

    const newMessage: PortalMessageDto = {
      id: crypto.randomUUID(),
      sender: 'Owner',
      senderName: 'You',
      body: body.body,
      sentAt: new Date().toISOString(),
      attachments: [],
    }

    if (!portalMessages[conv.id]) portalMessages[conv.id] = []
    portalMessages[conv.id].push(newMessage)

    conv.lastMessageAt = newMessage.sentAt
    if (conv.status === 'Resolved') conv.status = 'InProgress'

    dailyMessageCount[token] = count + 1

    return HttpResponse.json(newMessage, { status: 201 })
  }),

  // POST /api/v1/portal/consent
  http.post(`${BASE}/consent`, async ({ request }) => {
    await delay(100)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    const body = await request.json() as { consentVersion: string }
    consentGiven.add(token)

    return HttpResponse.json({
      acceptedAt: new Date().toISOString(),
      version: body.consentVersion,
    })
  }),

  // GET /api/v1/portal/export
  http.get(`${BASE}/export`, async ({ request }) => {
    await delay(300)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    const lines: string[] = [
      'DESERT PAWS CLINIC — MESSAGING EXPORT',
      `Generated: ${new Date().toLocaleString('en-AE', { timeZone: 'Asia/Dubai' })}`,
      '='.repeat(60),
      '',
    ]

    for (const conv of portalConversations) {
      lines.push(`CONVERSATION: ${conv.subject}`)
      lines.push(`Category: ${conv.category} | Status: ${conv.status}`)
      lines.push(`Created: ${new Date(conv.createdAt).toLocaleString('en-AE', { timeZone: 'Asia/Dubai' })}`)
      lines.push('-'.repeat(40))

      const messages = portalMessages[conv.id] ?? []
      for (const msg of messages) {
        const time = new Date(msg.sentAt).toLocaleString('en-AE', { timeZone: 'Asia/Dubai' })
        lines.push(`[${time}] ${msg.senderName ?? msg.sender}:`)
        lines.push(msg.body)
        lines.push('')
      }
      lines.push('='.repeat(60))
      lines.push('')
    }

    const content = lines.join('\n')
    return new HttpResponse(content, {
      status: 200,
      headers: {
        'Content-Type': 'text/plain; charset=utf-8',
        'Content-Disposition': 'attachment; filename="my-conversations.txt"',
      },
    })
  }),


  // GET /api/v1/portal/clinic-info
  http.get(`${BASE}/clinic-info`, async ({ request }) => {
    await delay(100)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    return HttpResponse.json({
      name: 'Desert Paws Veterinary Clinic',
      address: 'Al Wasl Road, Jumeirah, Dubai, UAE',
      phone: '+971 4 123 4567',
      openingHours: 'Sun–Thu 08:00–20:00, Fri 08:00–12:00',
    })
  }),

  // GET /api/v1/portal/pets
  http.get(`${BASE}/pets`, async ({ request }) => {
    await delay(100)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    return HttpResponse.json(MOCK_PORTAL_PETS)
  }),

  // GET /api/v1/portal/my-animals
  http.get(`${BASE}/my-animals`, async ({ request }) => {
    await delay(150)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    return HttpResponse.json(MOCK_PORTAL_ANIMALS)
  }),

  // GET /api/v1/portal/animals/:id/records
  http.get(`${BASE}/animals/:id/records`, async ({ params, request }) => {
    await delay(120)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    const records = MOCK_PORTAL_MEDICAL_RECORDS[params.id as string] ?? []
    return HttpResponse.json(records)
  }),

  // GET /api/v1/portal/animals/:id/vaccinations
  http.get(`${BASE}/animals/:id/vaccinations`, async ({ params, request }) => {
    await delay(120)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    const vaccinations = MOCK_PORTAL_VACCINATIONS[params.id as string] ?? []
    return HttpResponse.json(vaccinations)
  }),

  // GET /api/v1/portal/animals/:id/prescriptions
  http.get(`${BASE}/animals/:id/prescriptions`, async ({ params, request }) => {
    await delay(120)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    const prescriptions = MOCK_PORTAL_PRESCRIPTIONS[params.id as string] ?? []
    return HttpResponse.json(prescriptions)
  }),

  // GET /api/v1/portal/animals/:id/weight
  http.get(`${BASE}/animals/:id/weight`, async ({ params, request }) => {
    await delay(120)
    const token = getOwnerToken(request)
    if (!token) return new HttpResponse(null, { status: 401 })

    const weights = MOCK_PORTAL_WEIGHT_HISTORY[params.id as string] ?? []
    return HttpResponse.json(weights)
  }),
]
