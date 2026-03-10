'use client'

import { useCallback, useEffect, useRef, useState } from 'react'

// ─── SSE Event Types ──────────────────────────────────────────────────────────

export type SseEventType = 'new-message' | 'conversation-updated' | 'unread-count'

export interface NewMessageEvent {
  type: 'new-message'
  conversationId: string
  messageId: string
  senderName: string | null
  preview: string
  category: string
  ownerName: string
  patientName: string | null
}

export interface ConversationUpdatedEvent {
  type: 'conversation-updated'
  conversationId: string
  status: string
  unreadCount: number
}

export interface UnreadCountEvent {
  type: 'unread-count'
  total: number
}

export type MessagingSseEvent =
  | NewMessageEvent
  | ConversationUpdatedEvent
  | UnreadCountEvent

// ─── Hook state ──────────────────────────────────────────────────────────────

export interface MessagingSseState {
  unreadCount: number
  latestEvent: MessagingSseEvent | null
  isConnected: boolean
}

// ─── Hook ─────────────────────────────────────────────────────────────────────

const SSE_ENDPOINT = '/api/v1/messaging/sse'
const MAX_BACKOFF_MS = 30_000
const INITIAL_BACKOFF_MS = 1_000

/**
 * Connects to the messaging SSE stream.
 * Handles automatic reconnection with exponential backoff.
 * Safe to use in any client component — cleans up on unmount.
 */
export function useMessagingSse(): MessagingSseState {
  const [unreadCount, setUnreadCount] = useState(0)
  const [latestEvent, setLatestEvent] = useState<MessagingSseEvent | null>(null)
  const [isConnected, setIsConnected] = useState(false)

  const esRef = useRef<EventSource | null>(null)
  const backoffRef = useRef(INITIAL_BACKOFF_MS)
  const retryTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const mountedRef = useRef(true)

  const connect = useCallback(() => {
    if (!mountedRef.current) return

    // Clean up previous connection if any
    if (esRef.current) {
      esRef.current.close()
      esRef.current = null
    }

    const es = new EventSource(SSE_ENDPOINT, { withCredentials: true })
    esRef.current = es

    es.addEventListener('open', () => {
      if (!mountedRef.current) return
      setIsConnected(true)
      backoffRef.current = INITIAL_BACKOFF_MS // reset backoff on successful connect
    })

    es.addEventListener('new-message', (e: MessageEvent) => {
      if (!mountedRef.current) return
      try {
        const data = JSON.parse(e.data) as Omit<NewMessageEvent, 'type'>
        const event: NewMessageEvent = { type: 'new-message', ...data }
        setLatestEvent(event)
        setUnreadCount((prev) => prev + 1)
      } catch {
        // ignore malformed events
      }
    })

    es.addEventListener('conversation-updated', (e: MessageEvent) => {
      if (!mountedRef.current) return
      try {
        const data = JSON.parse(e.data) as Omit<ConversationUpdatedEvent, 'type'>
        const event: ConversationUpdatedEvent = { type: 'conversation-updated', ...data }
        setLatestEvent(event)
        // Sync unread count from conversation-updated events
        setUnreadCount((prev) => {
          // We get per-conversation unread; adjust total by the delta isn't possible here
          // so we just trust unread-count events for the total
          return prev
        })
      } catch {
        // ignore malformed events
      }
    })

    es.addEventListener('unread-count', (e: MessageEvent) => {
      if (!mountedRef.current) return
      try {
        const data = JSON.parse(e.data) as Omit<UnreadCountEvent, 'type'>
        const event: UnreadCountEvent = { type: 'unread-count', total: data.total }
        setLatestEvent(event)
        setUnreadCount(data.total)
      } catch {
        // ignore malformed events
      }
    })

    es.addEventListener('error', () => {
      if (!mountedRef.current) return
      setIsConnected(false)
      es.close()
      esRef.current = null

      // Exponential backoff reconnect
      const delay = Math.min(backoffRef.current, MAX_BACKOFF_MS)
      backoffRef.current = Math.min(backoffRef.current * 2, MAX_BACKOFF_MS)

      retryTimerRef.current = setTimeout(() => {
        if (mountedRef.current) connect()
      }, delay)
    })
  }, []) // stable — no deps

  useEffect(() => {
    mountedRef.current = true
    connect()

    return () => {
      mountedRef.current = false
      if (retryTimerRef.current) clearTimeout(retryTimerRef.current)
      if (esRef.current) {
        esRef.current.close()
        esRef.current = null
      }
    }
  }, [connect])

  return { unreadCount, latestEvent, isConnected }
}
