'use client'

import { useTranslations } from 'next-intl'
import { cn } from '@/lib/utils'
import { Badge } from '@/components/ui/badge'
import type { ConversationDto, MessageCategory, ConversationStatus } from '@/lib/api/messaging-types'

interface ConversationListItemProps {
  conversation: ConversationDto
  isSelected: boolean
  onClick: () => void
}

const CATEGORY_COLOR: Record<MessageCategory, string> = {
  MedicalUrgency: 'bg-red-100 text-red-700 border-red-200',
  PostOperativeFollowUp: 'bg-orange-100 text-orange-700 border-orange-200',
  MedicalQuestion: 'bg-blue-100 text-blue-700 border-blue-200',
  AppointmentRequest: 'bg-green-100 text-green-700 border-green-200',
  Administrative: 'bg-gray-100 text-gray-700 border-gray-200',
  Feedback: 'bg-purple-100 text-purple-700 border-purple-200',
  Other: 'bg-gray-100 text-gray-600 border-gray-200',
}

const STATUS_COLOR: Record<ConversationStatus, string> = {
  Open: 'bg-blue-100 text-blue-700',
  InProgress: 'bg-yellow-100 text-yellow-700',
  Resolved: 'bg-green-100 text-green-700',
  Closed: 'bg-gray-100 text-gray-600',
}

function formatRelativeDate(dateStr: string): string {
  const date = new Date(dateStr)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  if (diffMins < 60) return `${diffMins}m ago`
  const diffHours = Math.floor(diffMins / 60)
  if (diffHours < 24) return `${diffHours}h ago`
  const diffDays = Math.floor(diffHours / 24)
  return `${diffDays}d ago`
}

export function ConversationListItem({
  conversation,
  isSelected,
  onClick,
}: ConversationListItemProps) {
  const t = useTranslations('messaging')
  const isUrgency = conversation.category === 'MedicalUrgency'

  return (
    <button
      data-testid={`conversation-item-${conversation.id}`}
      onClick={onClick}
      className={cn(
        'w-full text-left px-4 py-3 border-b transition-all duration-200 ease-in-out hover:bg-muted/50',
        isSelected && 'bg-primary/5 border-l-2 border-l-primary shadow-sm',
        !isSelected && 'border-l-2 border-l-transparent',
        isUrgency && !isSelected && 'bg-red-50/60',
      )}
    >
      <div className="flex items-start justify-between gap-2 mb-1">
        <div className="flex items-center gap-2 min-w-0">
          {/* Unread dot */}
          {conversation.unreadCount > 0 && (
            <span
              data-testid={`unread-dot-${conversation.id}`}
              className="flex-shrink-0 h-2 w-2 rounded-full bg-primary animate-pulse-badge"
            />
          )}
          <span className="font-medium text-sm truncate">{conversation.ownerName}</span>
        </div>
        <span className="text-xs text-muted-foreground flex-shrink-0">
          {formatRelativeDate(conversation.lastMessageAt)}
        </span>
      </div>

      <p className="text-sm text-foreground truncate mb-2">{conversation.subject}</p>

      <div className="flex items-center gap-2 flex-wrap">
        {/* Category badge */}
        <Badge
          data-testid={`category-badge-${conversation.id}`}
          variant="outline"
          className={cn('text-xs px-1.5 py-0', CATEGORY_COLOR[conversation.category])}
        >
          {t(`category.${conversation.category}`)}
        </Badge>

        {/* Status badge */}
        <Badge
          data-testid={`status-badge-${conversation.id}`}
          variant="secondary"
          className={cn('text-xs px-1.5 py-0', STATUS_COLOR[conversation.status])}
        >
          {t(`status.${conversation.status}`)}
        </Badge>

        {/* Unread count */}
        {conversation.unreadCount > 0 && (
          <span
            data-testid={`unread-count-${conversation.id}`}
            className="ml-auto flex-shrink-0 h-5 min-w-5 rounded-full bg-primary text-primary-foreground text-xs flex items-center justify-center px-1 animate-pulse-badge"
          >
            {conversation.unreadCount}
          </span>
        )}
      </div>

      {/* Patient name if present */}
      {conversation.patientName && (
        <p className="text-xs text-muted-foreground mt-1">
          {conversation.patientName}
        </p>
      )}
    </button>
  )
}
