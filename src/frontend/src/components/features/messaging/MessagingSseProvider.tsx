'use client'

import React, { createContext, useContext, useEffect } from 'react'
import { toast } from 'sonner'
import { useRouter } from 'next/navigation'
import { useMessagingSse } from '@/hooks/use-messaging-sse'
import { usePushNotifications } from '@/hooks/use-push-notifications'
import type { MessagingSseState, NewMessageEvent } from '@/hooks/use-messaging-sse'

// ─── Context ──────────────────────────────────────────────────────────────────

const MessagingSseContext = createContext<MessagingSseState>({
  unreadCount: 0,
  latestEvent: null,
  isConnected: false,
})

export function useMessagingSseContext(): MessagingSseState {
  return useContext(MessagingSseContext)
}

// ─── Provider ────────────────────────────────────────────────────────────────

interface MessagingSseProviderProps {
  children: React.ReactNode
}

/**
 * Wraps the dashboard layout to provide SSE-based realtime messaging updates.
 * - Maintains unread count from SSE stream
 * - Shows toast notifications when new messages arrive
 * - Triggers browser push notifications for MedicalUrgency messages
 */
export function MessagingSseProvider({ children }: MessagingSseProviderProps) {
  const sseState = useMessagingSse()
  const { notifyUrgent } = usePushNotifications()
  const router = useRouter()

  const { latestEvent } = sseState

  // React to new-message SSE events
  useEffect(() => {
    if (!latestEvent || latestEvent.type !== 'new-message') return

    const event = latestEvent as NewMessageEvent
    const preview = event.preview.length > 80
      ? event.preview.slice(0, 80) + '…'
      : event.preview

    // Show a toast — click navigates to the conversation
    toast(`New message from ${event.ownerName}`, {
      description: preview,
      duration: 6000,
      action: {
        label: 'View',
        onClick: () => {
          router.push(`/messages?conversationId=${event.conversationId}`)
        },
      },
    })

    // Push notification for urgencies
    if (event.category === 'MedicalUrgency') {
      notifyUrgent(event)
    }
  }, [latestEvent, notifyUrgent, router])

  return (
    <MessagingSseContext.Provider value={sseState}>
      {children}
    </MessagingSseContext.Provider>
  )
}
