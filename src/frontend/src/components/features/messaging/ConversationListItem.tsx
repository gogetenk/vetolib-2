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
  MedicalUrgency: 'bg-red-100 text-red-700 ring-red-200',
  PostOperativeFollowUp: 'bg-orange-100 text-orange-700 ring-orange-200',
  MedicalQuestion: 'bg-blue-100 text-blue-700 ring-blue-200',
  AppointmentRequest: 'bg-green-100 text-green-700 ring-green-200',
  Administrative: 'bg-[#f4f6f9] text-[#061e44] ring-border/50',
  Feedback: 'bg-purple-100 text-purple-700 ring-purple-200',
  Other: 'bg-[#f4f6f9] text-muted-foreground ring-border/50',
}

const STATUS_COLOR: Record<ConversationStatus, string> = {
  Open: 'bg-blue-100 text-blue-700',
  InProgress: 'bg-yellow-100 text-yellow-700',
  Resolved: 'bg-green-100 text-green-700',
  Closed: 'bg-[#f4f6f9] text-muted-foreground',
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
        'w-full text-left px-4 py-3.5 border-b border-border/30 transition-all duration-200 ease-in-out hover:bg-[#f4f6f9]/50',
        isSelected && 'bg-[#eef2fd] border-l-[3px] border-l-[#303ef5]',
        !isSelected && 'border-l-[3px] border-l-transparent',
        isUrgency && !isSelected && 'bg-red-50/30 hover:bg-red-50/60',
      )}
    >
      <div className="flex items-start justify-between gap-2 mb-1.5">
        <div className="flex items-center gap-2 min-w-0">
          {conversation.unreadCount > 0 && (
            <span
              data-testid={`unread-dot-${conversation.id}`}
              className="flex-shrink-0 h-2 w-2 rounded-full bg-[#303ef5] animate-pulse-badge"
            />
          )}
          <span className="font-bold text-[14px] text-[#061e44] truncate">{conversation.ownerName}</span>
        </div>
        <span className="text-[11px] font-semibold text-muted-foreground flex-shrink-0">
          {formatRelativeDate(conversation.lastMessageAt)}
        </span>
      </div>

      <p className="text-[13px] text-muted-foreground truncate mb-2.5 leading-snug">{conversation.subject}</p>

      <div className="flex items-center gap-2 flex-wrap">
        <span
          data-testid={`category-badge-${conversation.id}`}
          className={cn('inline-flex items-center rounded-md px-2 py-0.5 text-[10px] font-bold ring-1 ring-inset', CATEGORY_COLOR[conversation.category])}
        >
          {t(`category.${conversation.category}`)}
        </span>

        <span
          data-testid={`status-badge-${conversation.id}`}
          className={cn('inline-flex items-center rounded-md px-2 py-0.5 text-[10px] font-bold', STATUS_COLOR[conversation.status])}
        >
          {t(`status.${conversation.status}`)}
        </span>

        {conversation.unreadCount > 0 && (
          <span
            data-testid={`unread-count-${conversation.id}`}
            className="ml-auto flex-shrink-0 h-5 min-w-5 rounded-full bg-[#303ef5] text-white text-[11px] font-bold flex items-center justify-center px-1.5 shadow-sm animate-pulse-badge"
          >
            {conversation.unreadCount}
          </span>
        )}
      </div>

      {conversation.patientName && (
        <p className="text-[11px] font-semibold text-muted-foreground mt-2 flex items-center gap-1.5">
          <span className="w-1 h-1 rounded-full bg-border" />
          {conversation.patientName}
        </p>
      )}
    </button>
  )
}
