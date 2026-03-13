'use client'

import { useTranslations } from 'next-intl'
import { ConversationListItem } from './ConversationListItem'
import type { ConversationDto } from '@/lib/api/messaging-types'

interface ConversationListProps {
  conversations: ConversationDto[]
  selectedId: string | null
  isLoading: boolean
  onSelect: (id: string) => void
}

export function ConversationList({
  conversations,
  selectedId,
  isLoading,
  onSelect,
}: ConversationListProps) {
  const t = useTranslations('messaging')

  if (isLoading) {
    return (
      <div data-testid="conversation-list-loading" className="space-y-0">
        {[1, 2, 3, 4, 5].map((i) => (
          <div
            key={i}
            className="px-4 py-3 border-b space-y-2"
          >
            <div className="h-4 bg-muted rounded w-1/2 animate-shimmer" />
            <div className="h-3 bg-muted rounded w-3/4 animate-shimmer" />
            <div className="h-3 bg-muted rounded w-1/4 animate-shimmer" />
          </div>
        ))}
      </div>
    )
  }

  if (conversations.length === 0) {
    return (
      <div
        data-testid="conversation-list-empty"
        className="flex flex-col items-center justify-center py-16 text-center text-muted-foreground"
      >
        <p className="text-sm">{t('no_conversations')}</p>
      </div>
    )
  }

  return (
    <div data-testid="conversation-list" className="flex flex-col">
      {conversations.map((conv) => (
        <ConversationListItem
          key={conv.id}
          conversation={conv}
          isSelected={selectedId === conv.id}
          onClick={() => onSelect(conv.id)}
        />
      ))}
    </div>
  )
}
