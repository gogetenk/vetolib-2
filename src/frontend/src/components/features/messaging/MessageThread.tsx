'use client'

import { useEffect, useRef } from 'react'
import { useTranslations } from 'next-intl'
import { MessageBubble } from './MessageBubble'
import { ConversationSummary } from './ConversationSummary'
import type { MessageDto } from '@/lib/api/messaging-types'

interface MessageThreadProps {
  messages: MessageDto[]
  aiSummary: string | null
  isLoadingSummary: boolean
  showSummary: boolean
  /** If true, internal notes are hidden (e.g. for owner portal) */
  hideInternalNotes?: boolean
}

export function MessageThread({
  messages,
  aiSummary,
  isLoadingSummary,
  showSummary,
  hideInternalNotes = false,
}: MessageThreadProps) {
  const t = useTranslations('messaging')
  const bottomRef = useRef<HTMLDivElement>(null)

  // Auto-scroll to the last message when messages change
  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages.length])

  const visibleMessages = hideInternalNotes
    ? messages.filter((m) => !m.isInternalNote)
    : messages

  return (
    <div className="flex flex-col h-full overflow-y-auto px-4 py-4" data-testid="message-thread">
      {/* AI Summary — visible for VetOrAdmin when thread > 5 messages */}
      {showSummary && (
        <ConversationSummary summary={aiSummary} isLoading={isLoadingSummary} />
      )}

      {visibleMessages.length === 0 ? (
        <div className="flex-1 flex items-center justify-center text-sm text-muted-foreground">
          {t('no_messages')}
        </div>
      ) : (
        visibleMessages.map((msg) => <MessageBubble key={msg.id} message={msg} />)
      )}

      {/* Scroll anchor */}
      <div ref={bottomRef} aria-hidden />
    </div>
  )
}
