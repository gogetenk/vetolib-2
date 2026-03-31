import { apiGet, apiPut } from './client'

export type NotificationType =
  | 'HealthAlert'
  | 'Reminder'
  | 'StockLow'
  | 'Message'
  | 'System'

export interface NotificationDto {
  id: string
  type: NotificationType
  title: string
  message: string
  createdAt: string
  isRead: boolean
  actionUrl: string | null
}

export interface NotificationInboxResponse {
  items: NotificationDto[]
  totalCount: number
  unreadCount: number
  page: number
  pageSize: number
}

const BASE = '/api/v1/notifications/inbox'

export function getNotificationInbox(
  page = 1,
  pageSize = 20
): Promise<NotificationInboxResponse> {
  return apiGet<NotificationInboxResponse>(
    `${BASE}?page=${page}&pageSize=${pageSize}`
  )
}

export function markNotificationRead(id: string): Promise<void> {
  return apiPut<void>(`${BASE}/${id}/read`, {})
}

export function markAllNotificationsRead(): Promise<void> {
  return apiPut<void>(`${BASE}/read-all`, {})
}
