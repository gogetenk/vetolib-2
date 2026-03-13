'use client'

import { useCallback, useEffect, useMemo, useState } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter } from 'next/navigation'
import { toast } from 'sonner'
import { ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { cn } from '@/lib/utils'
import { ConversationFilters } from './ConversationFilters'
import { ConversationList } from './ConversationList'
import { listConversations, changeStatus } from '@/lib/api/messaging'
import { useRole } from '@/hooks/use-role'
import type {
  ConversationDto,
  MessageCategory,
  ConversationStatus,
} from '@/lib/api/messaging-types'

// Role-based category access
const ROLE_CATEGORIES: Record<string, MessageCategory[]> = {
  RECEPTIONIST: ['AppointmentRequest', 'Administrative', 'Other'],
  VET: ['MedicalUrgency', 'PostOperativeFollowUp', 'MedicalQuestion'],
  ADMIN: [
    'MedicalUrgency',
    'PostOperativeFollowUp',
    'MedicalQuestion',
    'AppointmentRequest',
    'Administrative',
    'Feedback',
    'Other',
  ],
  ASSISTANT: [
    'AppointmentRequest',
    'Administrative',
    'Feedback',
    'Other',
  ],
}

const STATUS_COLOR: Record<ConversationStatus, string> = {
  Open: 'bg-blue-100 text-blue-700',
  InProgress: 'bg-yellow-100 text-yellow-700',
  Resolved: 'bg-green-100 text-green-700',
  Closed: 'bg-stone-100 text-stone-600',
}

const CATEGORY_COLOR: Record<MessageCategory, string> = {
  MedicalUrgency: 'bg-red-100 text-red-700 border-red-200',
  PostOperativeFollowUp: 'bg-orange-100 text-orange-700 border-orange-200',
  MedicalQuestion: 'bg-blue-100 text-blue-700 border-blue-200',
  AppointmentRequest: 'bg-green-100 text-green-700 border-green-200',
  Administrative: 'bg-stone-100 text-stone-700 border-stone-200',
  Feedback: 'bg-purple-100 text-purple-700 border-purple-200',
  Other: 'bg-stone-100 text-stone-600 border-stone-200',
}

function formatDate(dateStr: string): string {
  const date = new Date(dateStr)
  return new Intl.DateTimeFormat('en-AE', {
    timeZone: 'Asia/Dubai',
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(date)
}

export function MessagesPage() {
  const t = useTranslations('messaging')
  const role = useRole()
  const router = useRouter()

  const [conversations, setConversations] = useState<ConversationDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [showMobileDetail, setShowMobileDetail] = useState(false)

  // Filters
  const [statusFilter, setStatusFilter] = useState<ConversationStatus | ''>('')
  const [categoryFilter, setCategoryFilter] = useState<MessageCategory | ''>('')
  const [searchQuery, setSearchQuery] = useState('')

  const allowedCategories = useMemo(
    () => ROLE_CATEGORIES[role] ?? ROLE_CATEGORIES['ADMIN'],
    [role]
  )

  const fetchConversations = useCallback(async () => {
    setIsLoading(true)
    try {
      const result = await listConversations({
        status: statusFilter || undefined,
        category: categoryFilter || undefined,
        pageSize: 50,
      })
      // Filter by role-allowed categories
      const filtered = result.items.filter((c) =>
        allowedCategories.includes(c.category)
      )
      setConversations(filtered)
    } catch {
      toast.error(t('errors.load_failed'))
    } finally {
      setIsLoading(false)
    }
  }, [statusFilter, categoryFilter, allowedCategories, t])

  useEffect(() => {
    fetchConversations()
  }, [fetchConversations])

  // Client-side search filter
  const filteredConversations = useMemo(() => {
    if (!searchQuery.trim()) return conversations
    const q = searchQuery.toLowerCase()
    return conversations.filter(
      (c) =>
        c.subject.toLowerCase().includes(q) ||
        c.ownerName.toLowerCase().includes(q) ||
        (c.patientName?.toLowerCase().includes(q) ?? false)
    )
  }, [conversations, searchQuery])

  const selectedConversation = useMemo(
    () => filteredConversations.find((c) => c.id === selectedId) ?? null,
    [filteredConversations, selectedId]
  )

  const handleSelect = (id: string) => {
    setSelectedId(id)
    setShowMobileDetail(true)
    // Navigate to full detail page
    router.push(`/messages/${id}`)
  }

  const handleBackToList = () => {
    setShowMobileDetail(false)
  }

  const handleStatusChange = async (id: string, newStatus: ConversationStatus) => {
    if (role === 'ASSISTANT') return // read-only
    try {
      const updated = await changeStatus(id, { status: newStatus })
      setConversations((prev) =>
        prev.map((c) => (c.id === id ? { ...c, status: updated.status } : c))
      )
      toast.success(t('status_updated'))
    } catch {
      toast.error(t('errors.status_failed'))
    }
  }

  const totalUnread = conversations.reduce((sum, c) => sum + c.unreadCount, 0)

  return (
    <div className="flex flex-col h-full" data-testid="messages-page">
      {/* Page header */}
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-2">
          <h1 className="text-2xl font-bold" data-testid="messages-title">
            {t('title')}
          </h1>
          {totalUnread > 0 && (
            <Badge
              data-testid="unread-total-badge"
              data-pulse
              className="bg-primary text-primary-foreground"
            >
              {totalUnread}
            </Badge>
          )}
        </div>
      </div>

      {/* Filters */}
      <ConversationFilters
        statusFilter={statusFilter}
        categoryFilter={categoryFilter}
        searchQuery={searchQuery}
        onStatusChange={setStatusFilter}
        onCategoryChange={setCategoryFilter}
        onSearchChange={setSearchQuery}
      />

      {/* Main layout: list + detail */}
      <div className="mt-4 flex flex-1 border rounded-lg overflow-hidden min-h-0">
        {/* Conversation list — hidden on mobile when detail is shown */}
        <div
          className={cn(
            'w-full md:w-80 lg:w-96 border-r flex flex-col flex-shrink-0 overflow-y-auto',
            showMobileDetail ? 'hidden md:flex' : 'flex'
          )}
          data-testid="conversation-list-panel"
        >
          <ConversationList
            conversations={filteredConversations}
            selectedId={selectedId}
            isLoading={isLoading}
            onSelect={handleSelect}
          />
        </div>

        {/* Detail panel */}
        <div
          className={cn(
            'flex-1 flex flex-col overflow-y-auto',
            !showMobileDetail ? 'hidden md:flex' : 'flex'
          )}
          data-testid="conversation-detail-panel"
        >
          {selectedConversation ? (
            <ConversationDetail
              conversation={selectedConversation}
              role={role}
              onStatusChange={handleStatusChange}
              onBack={handleBackToList}
              t={t}
            />
          ) : (
            <div className="flex flex-1 items-center justify-center text-muted-foreground">
              <p className="text-sm">{t('select_conversation')}</p>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

// ─── Inline conversation detail ───────────────────────────────────────────────

interface ConversationDetailProps {
  conversation: ConversationDto
  role: string
  onStatusChange: (id: string, status: ConversationStatus) => void
  onBack: () => void
  t: ReturnType<typeof useTranslations<'messaging'>>
}

const ALL_STATUSES: ConversationStatus[] = ['Open', 'InProgress', 'Resolved', 'Closed']

function ConversationDetail({ conversation, role, onStatusChange, onBack, t }: ConversationDetailProps) {
  const isReadOnly = role === 'ASSISTANT'

  return (
    <div className="flex flex-col h-full">
      {/* Detail header */}
      <div className="border-b p-4 flex items-start gap-3">
        {/* Back button — mobile only */}
        <Button
          variant="ghost"
          size="sm"
          className="md:hidden -ml-1"
          data-testid="btn-back-to-list"
          onClick={onBack}
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>

        <div className="flex-1 min-w-0">
          <h2
            className="text-base font-semibold truncate"
            data-testid="detail-subject"
          >
            {conversation.subject}
          </h2>
          <div className="flex items-center gap-2 mt-1 flex-wrap">
            <span className="text-sm text-muted-foreground" data-testid="detail-owner">
              {conversation.ownerName}
            </span>
            {conversation.patientName && (
              <span className="text-sm text-muted-foreground">
                · {conversation.patientName}
              </span>
            )}
          </div>
          <div className="flex items-center gap-2 mt-2 flex-wrap">
            <Badge
              data-testid="detail-category-badge"
              variant="outline"
              className={cn('text-xs', CATEGORY_COLOR[conversation.category])}
            >
              {t(`category.${conversation.category}`)}
            </Badge>
            <Badge
              data-testid="detail-status-badge"
              variant="secondary"
              className={cn('text-xs', STATUS_COLOR[conversation.status])}
            >
              {t(`status.${conversation.status}`)}
            </Badge>
            {conversation.isTriageUncertain && (
              <Badge
                data-testid="detail-triage-uncertain-badge"
                variant="outline"
                className="text-xs bg-yellow-50 text-yellow-700 border-yellow-200"
              >
                {t('triage_uncertain')}
              </Badge>
            )}
          </div>
        </div>

        {/* Status change actions — not for assistant */}
        {!isReadOnly && (
          <div className="flex flex-col gap-1 items-end" data-testid="status-actions">
            {ALL_STATUSES.filter((s) => s !== conversation.status).map((s) => (
              <Button
                key={s}
                variant="outline"
                size="sm"
                data-testid={`btn-set-status-${s.toLowerCase()}`}
                onClick={() => onStatusChange(conversation.id, s)}
              >
                {t(`status.${s}`)}
              </Button>
            ))}
          </div>
        )}
      </div>

      {/* Metadata */}
      <div className="px-4 py-3 border-b bg-muted/30 text-xs text-muted-foreground flex flex-wrap gap-4">
        <span data-testid="detail-created-at">
          {t('created')}: {formatDate(conversation.createdAt)}
        </span>
        <span data-testid="detail-last-message-at">
          {t('last_message')}: {formatDate(conversation.lastMessageAt)}
        </span>
        {conversation.assignedToRole && (
          <span data-testid="detail-assigned-role">
            {t('assigned_to')}: {conversation.assignedToRole}
          </span>
        )}
        {conversation.aiTriageConfidence !== null && (
          <span data-testid="detail-ai-confidence">
            AI: {Math.round(conversation.aiTriageConfidence * 100)}%
          </span>
        )}
      </div>

      {/* Conversation body placeholder — messages are loaded in a separate task */}
      <div
        className="flex-1 flex items-center justify-center text-sm text-muted-foreground p-8 text-center"
        data-testid="detail-messages-placeholder"
      >
        <div>
          <p className="font-medium mb-1">{conversation.subject}</p>
          <p>{t('messages_coming_soon')}</p>
        </div>
      </div>
    </div>
  )
}
