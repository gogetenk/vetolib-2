import { http, HttpResponse, delay } from 'msw'
import type { NewMessageEvent, UnreadCountEvent, ConversationUpdatedEvent } from '@/hooks/use-messaging-sse'
import {
  MOCK_CONVERSATIONS,
  MOCK_MESSAGES,
  MOCK_AI_SUGGESTIONS,
  MOCK_AI_SUMMARIES,
  MOCK_TEMPLATES,
  MOCK_MESSAGING_HOURS,
} from '@/mocks/data/messaging'
import type {
  ConversationDto,
  ConversationWithSuggestionsDto,
  MessageDto,
  ResponseTemplateDto,
  MessagingHoursDto,
  MessageCategory,
  ConversationStatus,
} from '@/lib/api/messaging-types'
import type { PagedResult } from '@/lib/api/types'

// Mutable copies so mutations persist within the session
const conversations: ConversationDto[] = MOCK_CONVERSATIONS.map(c => ({ ...c }))
const messageStore: Record<string, MessageDto[]> = Object.fromEntries(
  Object.entries(MOCK_MESSAGES).map(([k, v]) => [k, v.map(m => ({ ...m }))])
)
const templateStore: ResponseTemplateDto[] = MOCK_TEMPLATES.map(t => ({ ...t }))
const messagingHours: MessagingHoursDto[] = MOCK_MESSAGING_HOURS.map(h => ({ ...h }))

const BASE = '/api/v1/messaging'

export const messagingHandlers = [
  // GET /api/v1/messaging/conversations
  http.get(`${BASE}/conversations`, async ({ request }) => {
    await delay(150)
    const url = new URL(request.url)
    const status = url.searchParams.get('status') as ConversationStatus | null
    const category = url.searchParams.get('category') as MessageCategory | null
    const assignedToUserId = url.searchParams.get('assignedToUserId')
    const page = parseInt(url.searchParams.get('page') ?? '1')
    const pageSize = parseInt(url.searchParams.get('pageSize') ?? '20')

    let items = conversations.filter(c => !c.isSpam)
    if (status) items = items.filter(c => c.status === status)
    if (category) items = items.filter(c => c.category === category)
    if (assignedToUserId) items = items.filter(c => c.assignedToUserId === assignedToUserId)

    // Sort: Critical first, then by lastMessageAt desc
    const priorityOrder: Record<string, number> = { Critical: 0, High: 1, Normal: 2, Low: 3 }
    items = [...items].sort((a, b) => {
      const pa = priorityOrder[a.priority] ?? 3
      const pb = priorityOrder[b.priority] ?? 3
      if (pa !== pb) return pa - pb
      return new Date(b.lastMessageAt).getTime() - new Date(a.lastMessageAt).getTime()
    })

    const start = (page - 1) * pageSize
    const paged = items.slice(start, start + pageSize)

    return HttpResponse.json<PagedResult<ConversationDto>>({
      items: paged,
      totalCount: items.length,
      page,
      pageSize,
    })
  }),

  // GET /api/v1/messaging/conversations/:id
  http.get(`${BASE}/conversations/:id`, async ({ params }) => {
    await delay(100)
    const conv = conversations.find(c => c.id === params.id)
    if (!conv) return new HttpResponse(null, { status: 404 })

    const messages = messageStore[conv.id] ?? []
    const suggestions = MOCK_AI_SUGGESTIONS[conv.id] ?? []
    const summary = messages.length > 5 ? (MOCK_AI_SUMMARIES[conv.id] ?? null) : null

    const result: ConversationWithSuggestionsDto = {
      ...conv,
      messages,
      aiSuggestions: suggestions,
      aiSummary: summary,
    }
    return HttpResponse.json(result)
  }),

  // POST /api/v1/messaging/conversations/:id/reply
  http.post(`${BASE}/conversations/:id/reply`, async ({ params, request }) => {
    await delay(200)
    const conv = conversations.find(c => c.id === params.id)
    if (!conv) return new HttpResponse(null, { status: 404 })

    const body = await request.json() as { body: string }
    const newMessage: MessageDto = {
      id: crypto.randomUUID(),
      conversationId: conv.id,
      sender: 'Staff',
      senderUserId: 'user-staff-current',
      senderName: 'Current User',
      body: body.body,
      isInternalNote: false,
      sentAt: new Date().toISOString(),
      attachments: [],
    }

    if (!messageStore[conv.id]) messageStore[conv.id] = []
    messageStore[conv.id].push(newMessage)

    conv.status = 'InProgress'
    conv.lastMessageAt = newMessage.sentAt
    conv.unreadCount = 0

    return HttpResponse.json(newMessage, { status: 201 })
  }),

  // POST /api/v1/messaging/conversations/:id/notes
  http.post(`${BASE}/conversations/:id/notes`, async ({ params, request }) => {
    await delay(200)
    const conv = conversations.find(c => c.id === params.id)
    if (!conv) return new HttpResponse(null, { status: 404 })

    const body = await request.json() as { body: string }
    const newNote: MessageDto = {
      id: crypto.randomUUID(),
      conversationId: conv.id,
      sender: 'Vet',
      senderUserId: 'user-vet-current',
      senderName: 'Current User',
      body: body.body,
      isInternalNote: true,
      sentAt: new Date().toISOString(),
      attachments: [],
    }

    if (!messageStore[conv.id]) messageStore[conv.id] = []
    messageStore[conv.id].push(newNote)

    return HttpResponse.json(newNote, { status: 201 })
  }),

  // PATCH /api/v1/messaging/conversations/:id/status
  http.patch(`${BASE}/conversations/:id/status`, async ({ params, request }) => {
    await delay(150)
    const conv = conversations.find(c => c.id === params.id)
    if (!conv) return new HttpResponse(null, { status: 404 })

    const body = await request.json() as { status: ConversationStatus }
    conv.status = body.status
    return HttpResponse.json(conv)
  }),

  // PATCH /api/v1/messaging/conversations/:id/transfer
  http.patch(`${BASE}/conversations/:id/transfer`, async ({ params, request }) => {
    await delay(150)
    const conv = conversations.find(c => c.id === params.id)
    if (!conv) return new HttpResponse(null, { status: 404 })

    const body = await request.json() as { toRole: string; toUserId?: string | null }
    conv.assignedToRole = body.toRole
    conv.assignedToUserId = body.toUserId ?? null
    conv.assignedToUserName = null

    // Add a system note in the message thread
    const transferNote: MessageDto = {
      id: crypto.randomUUID(),
      conversationId: conv.id,
      sender: 'System',
      senderUserId: null,
      senderName: 'System',
      body: `Conversation transferred to ${body.toRole}`,
      isInternalNote: true,
      sentAt: new Date().toISOString(),
      attachments: [],
    }
    if (!messageStore[conv.id]) messageStore[conv.id] = []
    messageStore[conv.id].push(transferNote)

    return HttpResponse.json(conv)
  }),

  // PATCH /api/v1/messaging/conversations/:id/category
  http.patch(`${BASE}/conversations/:id/category`, async ({ params, request }) => {
    await delay(150)
    const conv = conversations.find(c => c.id === params.id)
    if (!conv) return new HttpResponse(null, { status: 404 })

    const body = await request.json() as { category: MessageCategory }
    conv.category = body.category
    conv.isTriageUncertain = false

    const priorityMap: Record<MessageCategory, ConversationDto['priority']> = {
      MedicalUrgency: 'Critical',
      PostOperativeFollowUp: 'High',
      MedicalQuestion: 'Normal',
      AppointmentRequest: 'Normal',
      Administrative: 'Low',
      Feedback: 'Low',
      Other: 'Low',
    }
    conv.priority = priorityMap[body.category]

    return HttpResponse.json(conv)
  }),

  // POST /api/v1/messaging/conversations/:id/spam
  http.post(`${BASE}/conversations/:id/spam`, async ({ params }) => {
    await delay(100)
    const conv = conversations.find(c => c.id === params.id)
    if (!conv) return new HttpResponse(null, { status: 404 })

    conv.isSpam = true
    conv.status = 'Closed'
    return HttpResponse.json(conv)
  }),

  // POST /api/v1/messaging/conversations/outbound
  http.post(`${BASE}/conversations/outbound`, async ({ request }) => {
    await delay(250)
    const body = await request.json() as {
      ownerId: string
      ownerName: string
      patientId: string | null
      subject: string
      body: string
    }

    const newConv: ConversationDto = {
      id: crypto.randomUUID(),
      clinicId: 'clinic-001',
      ownerId: body.ownerId,
      ownerName: body.ownerName,
      ownerEmail: `${body.ownerId}@portal.vetolib.ae`,
      patientId: body.patientId,
      patientName: null,
      subject: body.subject,
      category: 'Other',
      priority: 'Low',
      status: 'Open',
      assignedToUserId: null,
      assignedToUserName: null,
      assignedToRole: 'ADMIN',
      aiTriageConfidence: null,
      isTriageUncertain: false,
      isSpam: false,
      unreadCount: 0,
      lastMessageAt: new Date().toISOString(),
      createdAt: new Date().toISOString(),
    }
    conversations.push(newConv)

    const firstMessage: MessageDto = {
      id: crypto.randomUUID(),
      conversationId: newConv.id,
      sender: 'Staff',
      senderUserId: 'user-admin-current',
      senderName: 'Admin',
      body: body.body,
      isInternalNote: false,
      sentAt: new Date().toISOString(),
      attachments: [],
    }
    messageStore[newConv.id] = [firstMessage]

    return HttpResponse.json(newConv, { status: 201 })
  }),

  // GET /api/v1/messaging/conversations/:id/summary
  http.get(`${BASE}/conversations/:id/summary`, async ({ params }) => {
    await delay(300) // Simulate AI processing delay
    const conv = conversations.find(c => c.id === params.id)
    if (!conv) return new HttpResponse(null, { status: 404 })

    const summary = MOCK_AI_SUMMARIES[params.id as string]
    if (!summary) {
      const messages = messageStore[params.id as string] ?? []
      if (messages.length <= 5) {
        return HttpResponse.json(
          { title: 'Conversation has 5 messages or fewer — summary not available' },
          { status: 422 }
        )
      }
    }

    return HttpResponse.json({
      summary: summary ?? 'AI-generated summary: This conversation involves a follow-up on a patient\'s condition. The owner has been in contact with the veterinary team regarding health concerns. Progress has been noted and the case is being monitored.',
    })
  }),

  // GET /api/v1/messaging/stats
  http.get(`${BASE}/stats`, async () => {
    await delay(200)
    return HttpResponse.json({
      averageFirstResponseMinutes: 38,
      totalConversations: 142,
      openConversations: 8,
      byCategory: [
        { category: 'MedicalUrgency', count: 12, percentage: 8.5 },
        { category: 'PostOperativeFollowUp', count: 18, percentage: 12.7 },
        { category: 'MedicalQuestion', count: 35, percentage: 24.6 },
        { category: 'AppointmentRequest', count: 42, percentage: 29.6 },
        { category: 'Administrative', count: 25, percentage: 17.6 },
        { category: 'Feedback', count: 7, percentage: 4.9 },
        { category: 'Other', count: 3, percentage: 2.1 },
      ],
      aiAccuracyPercent: 91.5,
      conversionToAppointmentPercent: 34.2,
      dailyVolume: [
        { date: '2026-03-04', count: 18 },
        { date: '2026-03-05', count: 22 },
        { date: '2026-03-06', count: 9 },
        { date: '2026-03-08', count: 25 },
        { date: '2026-03-09', count: 31 },
        { date: '2026-03-10', count: 14 },
      ],
    })
  }),

  // GET /api/v1/messaging/templates
  http.get(`${BASE}/templates`, async () => {
    await delay(100)
    return HttpResponse.json(templateStore)
  }),

  // POST /api/v1/messaging/templates
  http.post(`${BASE}/templates`, async ({ request }) => {
    await delay(200)
    const body = await request.json() as {
      name: string
      contentEn: string
      contentAr: string
      category: MessageCategory | null
    }
    const newTemplate: ResponseTemplateDto = {
      id: crypto.randomUUID(),
      clinicId: 'clinic-001',
      name: body.name,
      contentEn: body.contentEn,
      contentAr: body.contentAr,
      category: body.category,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    }
    templateStore.push(newTemplate)
    return HttpResponse.json(newTemplate, { status: 201 })
  }),

  // PUT /api/v1/messaging/templates/:id
  http.put(`${BASE}/templates/:id`, async ({ params, request }) => {
    await delay(200)
    const idx = templateStore.findIndex(t => t.id === params.id)
    if (idx === -1) return new HttpResponse(null, { status: 404 })

    const body = await request.json() as {
      name: string
      contentEn: string
      contentAr: string
      category: MessageCategory | null
    }
    templateStore[idx] = {
      ...templateStore[idx],
      name: body.name,
      contentEn: body.contentEn,
      contentAr: body.contentAr,
      category: body.category,
      updatedAt: new Date().toISOString(),
    }
    return HttpResponse.json(templateStore[idx])
  }),

  // DELETE /api/v1/messaging/templates/:id
  http.delete(`${BASE}/templates/:id`, async ({ params }) => {
    await delay(150)
    const idx = templateStore.findIndex(t => t.id === params.id)
    if (idx === -1) return new HttpResponse(null, { status: 404 })
    templateStore.splice(idx, 1)
    return new HttpResponse(null, { status: 204 })
  }),

  // GET /api/v1/messaging/hours
  http.get(`${BASE}/hours`, async () => {
    await delay(100)
    return HttpResponse.json(messagingHours)
  }),

  // PATCH /api/v1/messaging/hours
  http.patch(`${BASE}/hours`, async ({ request }) => {
    await delay(200)
    const body = await request.json() as MessagingHoursDto[]
    body.forEach(updated => {
      const idx = messagingHours.findIndex(h => h.dayOfWeek === updated.dayOfWeek)
      if (idx !== -1) messagingHours[idx] = { ...updated }
    })
    return HttpResponse.json(messagingHours)
  }),

  // GET /api/v1/messaging/patients/:patientId/context
  http.get(`${BASE}/patients/:patientId/context`, async ({ params }) => {
    await delay(150)
    const patientId = params.patientId as string

    // Mock patient context — indexed by patientId
    const MOCK_PATIENT_CONTEXTS: Record<string, object> = {
      'pat-0000-0000-0000-000000000001': {
        patientId: 'pat-0000-0000-0000-000000000001',
        patientName: 'Max',
        species: 'Dog',
        breed: 'Golden Retriever',
        ageYears: 6,
        lastExaminationDate: new Date('2026-02-14T09:00:00+04:00').toISOString(),
        currentPrescriptions: ['Rimadyl 100mg — 1x daily', 'Omega-3 supplement'],
        knownAllergies: [],
        vaccinationHistory: ['Rabies (Feb 2025)', 'DHPP (Feb 2025)', 'Bordetella (Jan 2025)'],
        outstandingInvoicesAed: 0,
      },
      'pat-0000-0000-0000-000000000002': {
        patientId: 'pat-0000-0000-0000-000000000002',
        patientName: 'Luna',
        species: 'Cat',
        breed: 'Persian',
        ageYears: 3,
        lastExaminationDate: new Date('2026-03-05T14:00:00+04:00').toISOString(),
        currentPrescriptions: ['Amoxicillin 50mg — 2x daily (until 2026-03-16)', 'Chlorhexidine wash'],
        knownAllergies: ['Penicillin'],
        vaccinationHistory: ['FVRCP (Nov 2025)', 'Rabies (Nov 2025)'],
        outstandingInvoicesAed: 450,
      },
      'pat-0000-0000-0000-000000000003': {
        patientId: 'pat-0000-0000-0000-000000000003',
        patientName: 'Simba',
        species: 'Cat',
        breed: 'Tabby',
        ageYears: 2,
        lastExaminationDate: new Date('2026-01-10T10:00:00+04:00').toISOString(),
        currentPrescriptions: [],
        knownAllergies: [],
        vaccinationHistory: ['FVRCP (Jan 2025)'],
        outstandingInvoicesAed: 0,
      },
      'pat-0000-0000-0000-000000000004': {
        patientId: 'pat-0000-0000-0000-000000000004',
        patientName: 'Bella',
        species: 'Dog',
        breed: 'Labrador',
        ageYears: 4,
        lastExaminationDate: new Date('2026-02-28T11:00:00+04:00').toISOString(),
        currentPrescriptions: [],
        knownAllergies: [],
        vaccinationHistory: ['Rabies (Feb 2026)', 'DHPP (Feb 2026)'],
        outstandingInvoicesAed: 200,
      },
    }

    const context = MOCK_PATIENT_CONTEXTS[patientId]
    if (!context) return new HttpResponse(null, { status: 404 })
    return HttpResponse.json(context)
  }),

  // GET /api/v1/messaging/whatsapp/config
  http.get(`${BASE}/whatsapp/config`, async () => {
    await delay(150)
    return HttpResponse.json({
      enabled: false,
      businessAccountId: '',
      phoneNumberId: '',
      accessToken: '',
      optInCount: 27,
    })
  }),

  // PUT /api/v1/messaging/whatsapp/config
  http.put(`${BASE}/whatsapp/config`, async ({ request }) => {
    await delay(200)
    const body = await request.json() as {
      enabled: boolean
      businessAccountId: string
      phoneNumberId: string
      accessToken: string
    }
    return HttpResponse.json({
      enabled: body.enabled,
      businessAccountId: body.businessAccountId,
      phoneNumberId: body.phoneNumberId,
      accessToken: body.accessToken,
      optInCount: 27,
    })
  }),

  // POST /api/v1/messaging/whatsapp/test
  http.post(`${BASE}/whatsapp/test`, async () => {
    await delay(500)
    return HttpResponse.json({
      success: true,
      message: 'Template sent to +971-50-123-4567',
    })
  }),

  // GET /api/v1/messaging/sse — SSE mock stream
  // MSW intercepts the EventSource request. We return a text/event-stream ReadableStream
  // that emits mock events every 60 seconds so the SSE hook stays exercised in dev.
  http.get(`${BASE}/sse`, () => {
    let intervalId: ReturnType<typeof setInterval> | null = null

    const stream = new ReadableStream({
      start(controller) {
        const encode = (text: string) => new TextEncoder().encode(text)

        // Emit initial unread-count event immediately
        const initialCount: UnreadCountEvent = { type: 'unread-count', total: 3 }
        controller.enqueue(encode(`event: unread-count\ndata: ${JSON.stringify({ total: initialCount.total })}\n\n`))

        // Cycle through mock events every 60 s (throttled to avoid navigation storms in dev)
        let tick = 0
        intervalId = setInterval(() => {
          tick++
          try {
            if (tick % 3 === 1) {
              // new-message event (normal)
              const evt: Omit<NewMessageEvent, 'type'> = {
                conversationId: MOCK_CONVERSATIONS[0]?.id ?? 'conv-001',
                messageId: crypto.randomUUID(),
                senderName: null,
                preview: 'My dog has been limping since yesterday, is this an emergency?',
                category: 'MedicalQuestion',
                ownerName: 'Sara Al Mansouri',
                patientName: 'Noor',
              }
              controller.enqueue(encode(`event: new-message\ndata: ${JSON.stringify(evt)}\n\n`))
            } else if (tick % 3 === 2) {
              // new-message event (MedicalUrgency — triggers push notification)
              const urgentEvt: Omit<NewMessageEvent, 'type'> = {
                conversationId: MOCK_CONVERSATIONS[1]?.id ?? 'conv-002',
                messageId: crypto.randomUUID(),
                senderName: null,
                preview: 'He cannot breathe properly — URGENT!',
                category: 'MedicalUrgency',
                ownerName: 'Ahmed Al Rashidi',
                patientName: 'Barq',
              }
              controller.enqueue(encode(`event: new-message\ndata: ${JSON.stringify(urgentEvt)}\n\n`))
            } else {
              // unread-count refresh
              const countEvt: Omit<UnreadCountEvent, 'type'> = { total: tick + 3 }
              controller.enqueue(encode(`event: unread-count\ndata: ${JSON.stringify(countEvt)}\n\n`))
            }

            // Emit conversation-updated only on every 3rd tick to reduce state churn
            if (tick % 3 === 0) {
              const updatedEvt: Omit<ConversationUpdatedEvent, 'type'> = {
                conversationId: MOCK_CONVERSATIONS[0]?.id ?? 'conv-001',
                status: 'InProgress',
                unreadCount: tick,
              }
              controller.enqueue(encode(`event: conversation-updated\ndata: ${JSON.stringify(updatedEvt)}\n\n`))
            }
          } catch {
            // Stream may have been cancelled — stop
            if (intervalId !== null) clearInterval(intervalId)
          }
        }, 60_000)
      },
      cancel() {
        if (intervalId !== null) clearInterval(intervalId)
      },
    })

    return new HttpResponse(stream, {
      headers: {
        'Content-Type': 'text/event-stream',
        'Cache-Control': 'no-cache',
        'Connection': 'keep-alive',
      },
    })
  }),
]
