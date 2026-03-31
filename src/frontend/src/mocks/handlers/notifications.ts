import { http, HttpResponse, delay } from 'msw'
import type { NotificationDto, NotificationInboxResponse } from '@/lib/api/notifications'

const now = new Date()

function hoursAgo(h: number): string {
  return new Date(now.getTime() - h * 60 * 60 * 1000).toISOString()
}

const notifications: NotificationDto[] = [
  {
    id: 'notif-001',
    type: 'HealthAlert',
    title: 'Vaccination overdue — Luna',
    message: 'Luna (Persian, owner: Fatima Al Zahra) is 2 weeks overdue for FVRCP booster.',
    createdAt: hoursAgo(0.5),
    isRead: false,
    actionUrl: '/patients/pat-0000-0000-0000-000000000002',
  },
  {
    id: 'notif-002',
    type: 'StockLow',
    title: 'Low stock: Rimadyl 100mg',
    message: 'Only 3 units remaining. Reorder recommended before end of week.',
    createdAt: hoursAgo(1),
    isRead: false,
    actionUrl: '/stock',
  },
  {
    id: 'notif-003',
    type: 'Reminder',
    title: 'Follow-up due — Max',
    message: 'Post-operative check for Max (Golden Retriever) scheduled today at 14:00.',
    createdAt: hoursAgo(2),
    isRead: false,
    actionUrl: '/appointments',
  },
  {
    id: 'notif-004',
    type: 'Message',
    title: 'New message from Ahmed Al Rashidi',
    message: 'Regarding Barq — "He is not eating since yesterday, should I bring him in?"',
    createdAt: hoursAgo(3),
    isRead: false,
    actionUrl: '/messages',
  },
  {
    id: 'notif-005',
    type: 'System',
    title: 'Billing report ready',
    message: 'Your March 2026 billing summary is available for download.',
    createdAt: hoursAgo(6),
    isRead: true,
    actionUrl: '/billing',
  },
  {
    id: 'notif-006',
    type: 'HealthAlert',
    title: 'Weight alert — Bella',
    message: 'Bella (Labrador) has gained 2.3kg in the last month. Review diet plan recommended.',
    createdAt: hoursAgo(12),
    isRead: true,
    actionUrl: '/patients/pat-0000-0000-0000-000000000004',
  },
  {
    id: 'notif-007',
    type: 'Reminder',
    title: 'Annual checkup — Simba',
    message: 'Simba (Tabby, owner: Omar Hassan) is due for annual wellness exam next week.',
    createdAt: hoursAgo(24),
    isRead: true,
    actionUrl: '/patients/pat-0000-0000-0000-000000000003',
  },
  {
    id: 'notif-008',
    type: 'System',
    title: 'System maintenance scheduled',
    message: 'Planned maintenance on April 2, 2026 from 02:00-04:00 GST. Expect brief downtime.',
    createdAt: hoursAgo(48),
    isRead: true,
    actionUrl: null,
  },
]

const BASE = '/api/v1/notifications/inbox'

export const notificationHandlers = [
  // GET /api/v1/notifications/inbox
  http.get(BASE, async ({ request }) => {
    await delay(150)
    const url = new URL(request.url)
    const page = parseInt(url.searchParams.get('page') ?? '1')
    const pageSize = parseInt(url.searchParams.get('pageSize') ?? '20')

    const sorted = [...notifications].sort(
      (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    )
    const start = (page - 1) * pageSize
    const paged = sorted.slice(start, start + pageSize)
    const unreadCount = notifications.filter((n) => !n.isRead).length

    return HttpResponse.json<NotificationInboxResponse>({
      items: paged,
      totalCount: notifications.length,
      unreadCount,
      page,
      pageSize,
    })
  }),

  // PUT /api/v1/notifications/inbox/:id/read
  http.put(`${BASE}/:id/read`, async ({ params }) => {
    await delay(100)
    const notif = notifications.find((n) => n.id === params.id)
    if (!notif) return new HttpResponse(null, { status: 404 })
    notif.isRead = true
    return new HttpResponse(null, { status: 204 })
  }),

  // PUT /api/v1/notifications/inbox/read-all
  http.put(`${BASE}/read-all`, async () => {
    await delay(100)
    notifications.forEach((n) => {
      n.isRead = true
    })
    return new HttpResponse(null, { status: 204 })
  }),
]
