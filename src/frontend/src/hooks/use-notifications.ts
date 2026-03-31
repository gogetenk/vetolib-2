"use client"

import { useCallback, useEffect, useState } from 'react'
import type { NotificationDto } from '@/lib/api/notifications'
import {
  getNotificationInbox,
  markNotificationRead,
  markAllNotificationsRead,
} from '@/lib/api/notifications'

export function useNotifications() {
  const [notifications, setNotifications] = useState<NotificationDto[]>([])
  const [unreadCount, setUnreadCount] = useState(0)
  const [loading, setLoading] = useState(true)

  const fetchNotifications = useCallback(async () => {
    try {
      setLoading(true)
      const data = await getNotificationInbox(1, 20)
      setNotifications(data.items)
      setUnreadCount(data.unreadCount)
    } catch {
      // Silently fail — notification center is non-critical
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchNotifications()
  }, [fetchNotifications])

  const markRead = useCallback(
    async (id: string) => {
      try {
        await markNotificationRead(id)
        setNotifications((prev) =>
          prev.map((n) => (n.id === id ? { ...n, isRead: true } : n))
        )
        setUnreadCount((prev) => Math.max(0, prev - 1))
      } catch {
        // Silently fail
      }
    },
    []
  )

  const markAllRead = useCallback(async () => {
    try {
      await markAllNotificationsRead()
      setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })))
      setUnreadCount(0)
    } catch {
      // Silently fail
    }
  }, [])

  return {
    notifications,
    unreadCount,
    loading,
    markRead,
    markAllRead,
    refetch: fetchNotifications,
  }
}
