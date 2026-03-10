'use client'

import { useCallback, useEffect, useRef } from 'react'
import { useRouter } from 'next/navigation'
import type { NewMessageEvent } from '@/hooks/use-messaging-sse'

/**
 * Manages browser Push Notification permission and dispatches notifications
 * for urgent messages (MedicalUrgency category).
 *
 * Call `requestPermission()` after user login.
 * Call `notifyUrgent(event)` when a new-message SSE event arrives with category MedicalUrgency.
 */
export function usePushNotifications() {
  const router = useRouter()
  const permissionRef = useRef<NotificationPermission>('default')

  useEffect(() => {
    if (typeof Notification !== 'undefined') {
      permissionRef.current = Notification.permission
    }
  }, [])

  const requestPermission = useCallback(async () => {
    if (typeof Notification === 'undefined') return
    if (Notification.permission === 'granted') {
      permissionRef.current = 'granted'
      return
    }
    if (Notification.permission !== 'denied') {
      const result = await Notification.requestPermission()
      permissionRef.current = result
    }
  }, [])

  const notifyUrgent = useCallback(
    (event: NewMessageEvent) => {
      if (typeof Notification === 'undefined') return
      if (permissionRef.current !== 'granted') return
      if (event.category !== 'MedicalUrgency') return

      const title = 'URGENT - Emergency message'
      const body = [event.patientName, event.preview].filter(Boolean).join(' — ')

      try {
        const notification = new Notification(title, {
          body,
          icon: '/favicon.ico',
          tag: event.conversationId, // prevents duplicate notifications for same conversation
          requireInteraction: true,  // stays until user interacts
        })

        notification.onclick = () => {
          window.focus()
          router.push(`/messages?conversationId=${event.conversationId}`)
          notification.close()
        }
      } catch {
        // Notification API may be unavailable in some environments (e.g. iframes)
      }
    },
    [router]
  )

  return { requestPermission, notifyUrgent }
}
