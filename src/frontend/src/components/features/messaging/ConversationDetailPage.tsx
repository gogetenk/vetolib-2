'use client'

import { useCallback, useEffect, useState } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter } from 'next/navigation'
import { toast } from 'sonner'
import { ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { cn } from '@/lib/utils'
import { useRole } from '@/hooks/use-role'
import {
  getConversation,
  sendReply,
  addNote,
  changeStatus,
  transferConversation,
  markAsSpam,
} from '@/lib/api/messaging'
import { MessageThread } from './MessageThread'
import { AiSuggestionsPanel } from './AiSuggestionsPanel'
import { PatientContextPanel } from './PatientContextPanel'
import { ReplyComposer } from './ReplyComposer'
import { ConversationActions } from './ConversationActions'
import type {
  ConversationWithSuggestionsDto,
  MessageDto,
  ConversationStatus,
} from '@/lib/api/messaging-types'

const STATUS_COLOR: Record<ConversationStatus, string> = {
  Open: 'bg-blue-100 text-blue-700',
  InProgress: 'bg-yellow-100 text-yellow-700',
  Resolved: 'bg-green-100 text-green-700',
  Closed: 'bg-[#f4f6f9] text-muted-foreground',
}

const CATEGORY_COLOR: Record<string, string> = {
  MedicalUrgency: 'bg-red-100 text-red-700 border-red-200',
  PostOperativeFollowUp: 'bg-orange-100 text-orange-700 border-orange-200',
  MedicalQuestion: 'bg-blue-100 text-blue-700 border-blue-200',
  AppointmentRequest: 'bg-green-100 text-green-700 border-green-200',
  Administrative: 'bg-[#f4f6f9] text-[#061e44] border-border/50',
  Feedback: 'bg-purple-100 text-purple-700 border-purple-200',
  Other: 'bg-[#f4f6f9] text-muted-foreground border-border/50',
}

interface ConversationDetailPageProps {
  conversationId: string
}

export function ConversationDetailPage({ conversationId }: ConversationDetailPageProps) {
  const t = useTranslations('messaging')
  const role = useRole()
  const router = useRouter()

  const [conversation, setConversation] = useState<ConversationWithSuggestionsDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isSending, setIsSending] = useState(false)

  const isVetOrAdmin = role === 'VET' || role === 'ADMIN'
  const isAssistant = role === 'ASSISTANT'

  const fetchConversation = useCallback(async () => {
    setIsLoading(true)
    try {
      const data = await getConversation(conversationId)
      setConversation(data)
    } catch {
      toast.error(t('errors.load_failed'))
    } finally {
      setIsLoading(false)
    }
  }, [conversationId, t])

  useEffect(() => {
    fetchConversation()
  }, [fetchConversation])

  const handleSendReply = async (body: string) => {
    if (!conversation || isAssistant) return
    setIsSending(true)
    try {
      const newMsg = await sendReply(conversation.id, { body })
      setConversation((prev) =>
        prev
          ? {
              ...prev,
              messages: [...(prev.messages ?? []), newMsg],
              status: 'InProgress',
              lastMessageAt: newMsg.sentAt,
            }
          : prev
      )
      toast.success(t('reply_sent'))
    } catch {
      toast.error(t('errors.send_failed'))
    } finally {
      setIsSending(false)
    }
  }

  const handleAddNote = async (body: string) => {
    if (!conversation || !isVetOrAdmin) return
    setIsSending(true)
    try {
      const newNote = await addNote(conversation.id, { body })
      setConversation((prev) =>
        prev
          ? { ...prev, messages: [...(prev.messages ?? []), newNote] }
          : prev
      )
      toast.success(t('note_added'))
    } catch {
      toast.error(t('errors.send_failed'))
    } finally {
      setIsSending(false)
    }
  }

  const handleStatusChange = async (newStatus: ConversationStatus) => {
    if (!conversation || isAssistant) return
    try {
      const updated = await changeStatus(conversation.id, { status: newStatus })
      setConversation((prev) => prev ? { ...prev, status: updated.status } : prev)
      toast.success(t('status_updated'))
    } catch {
      toast.error(t('errors.status_failed'))
    }
  }

  const handleTransfer = async (toRole: string, toUserId?: string | null) => {
    if (!conversation || isAssistant) return
    try {
      const updated = await transferConversation(conversation.id, { toRole, toUserId })
      setConversation((prev) =>
        prev
          ? {
              ...prev,
              assignedToRole: updated.assignedToRole,
              assignedToUserId: updated.assignedToUserId,
              assignedToUserName: updated.assignedToUserName,
            }
          : prev
      )
      // Refresh to get the system message that was added
      await fetchConversation()
      toast.success(t('transferred'))
    } catch {
      toast.error(t('errors.transfer_failed'))
    }
  }

  const handleMarkSpam = async () => {
    if (!conversation || isAssistant) return
    try {
      await markAsSpam(conversation.id)
      toast.success(t('marked_spam'))
      router.back()
    } catch {
      toast.error(t('errors.spam_failed'))
    }
  }

  const handleConvertToAppointment = () => {
    if (!conversation) return
    router.push(`/messages?new-appointment=1&conversationId=${conversation.id}`)
  }

  const handleSuggestionSelect = (text: string) => {
    // The ReplyComposer receives the prefilled text via state lifted up
    setSuggestedText(text)
  }

  const [suggestedText, setSuggestedText] = useState('')

  if (isLoading) {
    return <ConversationDetailSkeleton />
  }

  if (!conversation) {
    return (
      <div
        className="flex flex-col items-center justify-center h-64 text-muted-foreground"
        data-testid="conversation-not-found"
      >
        <p>{t('errors.not_found')}</p>
        <Button
          variant="ghost"
          className="mt-4"
          onClick={() => router.back()}
          data-testid="btn-back-not-found"
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          {t('back_to_inbox')}
        </Button>
      </div>
    )
  }

  const messages: MessageDto[] = conversation.messages ?? []
  const showSummary = isVetOrAdmin && messages.length > 5

  return (
    <div className="flex flex-col h-full" data-testid="conversation-detail-page">
      {/* ── Header ─────────────────────────────────────────────────────────── */}
      <div className="border-b border-border/50 p-4 flex items-start gap-3 flex-shrink-0 bg-white">
        <Button
          variant="ghost"
          size="sm"
          className="-ml-1"
          data-testid="btn-back-to-inbox"
          onClick={() => router.back()}
          aria-label={t('back_to_inbox')}
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>

        <div className="flex-1 min-w-0">
          <h1
            className="text-[18px] font-bold text-[#061e44] truncate"
            data-testid="conversation-subject"
          >
            {conversation.subject}
          </h1>
          <div className="flex items-center gap-2 mt-1 flex-wrap">
            <span
              className="text-[13px] text-muted-foreground"
              data-testid="conversation-owner"
            >
              {conversation.ownerName}
            </span>
            {conversation.patientName && (
              <span className="text-[13px] text-muted-foreground" data-testid="conversation-patient">
                · {conversation.patientName}
              </span>
            )}
          </div>
          <div className="flex items-center gap-2 mt-2 flex-wrap">
            <Badge
              data-testid="conversation-category-badge"
              variant="outline"
              className={cn('text-[10px] font-bold rounded-md', CATEGORY_COLOR[conversation.category])}
            >
              {t(`category.${conversation.category}`)}
            </Badge>
            <Badge
              data-testid="conversation-status-badge"
              variant="secondary"
              className={cn('text-[10px] font-bold rounded-md', STATUS_COLOR[conversation.status])}
            >
              {t(`status.${conversation.status}`)}
            </Badge>
            {conversation.isTriageUncertain && (
              <Badge
                data-testid="conversation-triage-uncertain-badge"
                variant="outline"
                className="text-[10px] font-bold rounded-md bg-yellow-50 text-yellow-700 border-yellow-200"
              >
                {t('triage_uncertain')}
              </Badge>
            )}
            {conversation.assignedToUserName && (
              <span
                className="text-[12px] text-muted-foreground font-medium"
                data-testid="conversation-assigned-to"
              >
                {t('assigned_to')}: {conversation.assignedToUserName}
              </span>
            )}
          </div>
        </div>

        {/* Action toolbar */}
        {!isAssistant && (
          <ConversationActions
            conversation={conversation}
            role={role}
            onStatusChange={handleStatusChange}
            onTransfer={handleTransfer}
            onMarkSpam={handleMarkSpam}
            onConvertToAppointment={handleConvertToAppointment}
          />
        )}
      </div>

      {/* ── 3-column layout ────────────────────────────────────────────── */}
      <div className="flex flex-1 min-h-0 overflow-hidden">
        {/* Left: message thread + reply composer */}
        <div className="flex flex-col flex-1 min-w-0 overflow-hidden">
          {/* Thread */}
          <div className="flex-1 overflow-y-auto">
            <MessageThread
              messages={messages}
              aiSummary={conversation.aiSummary ?? null}
              isLoadingSummary={false}
              showSummary={showSummary}
            />
          </div>

          {/* Reply composer */}
          {!isAssistant && (
            <div className="border-t border-border/50 flex-shrink-0 bg-white">
              <ReplyComposer
                prefillText={suggestedText}
                onPrefillConsumed={() => setSuggestedText('')}
                isSending={isSending}
                canAddNote={isVetOrAdmin}
                canAttachToRecord={isVetOrAdmin}
                onSendReply={handleSendReply}
                onAddNote={handleAddNote}
              />
            </div>
          )}
        </div>

        {/* Right: context + suggestions (desktop) */}
        <div
          className="hidden lg:flex flex-col w-72 xl:w-80 border-l border-border/50 flex-shrink-0 overflow-y-auto bg-[#f4f6f9]"
          data-testid="right-panel"
        >
          {/* Patient context */}
          <PatientContextPanel
            patientId={conversation.patientId}
            patientName={conversation.patientName}
            role={role}
          />

          {/* AI suggestions */}
          {!isAssistant && conversation.aiSuggestions.length > 0 && (
            <AiSuggestionsPanel
              suggestions={conversation.aiSuggestions}
              onSelect={handleSuggestionSelect}
            />
          )}
        </div>
      </div>

      {/* Mobile: patient context + suggestions (below thread) */}
      <div className="lg:hidden border-t border-border/50">
        <PatientContextPanel
          patientId={conversation.patientId}
          patientName={conversation.patientName}
          role={role}
          compact
        />
        {!isAssistant && conversation.aiSuggestions.length > 0 && (
          <div className="border-t">
            <AiSuggestionsPanel
              suggestions={conversation.aiSuggestions}
              onSelect={handleSuggestionSelect}
            />
          </div>
        )}
      </div>
    </div>
  )
}

// ── Skeleton ──────────────────────────────────────────────────────────────────

function ConversationDetailSkeleton() {
  return (
    <div className="flex flex-col h-full" data-testid="conversation-detail-skeleton">
      <div className="border-b border-border/80 p-4 flex items-start gap-3">
        <Skeleton className="h-8 w-8 rounded-xl" />
        <div className="flex-1 space-y-2">
          <Skeleton className="h-5 w-3/4 rounded-xl" />
          <Skeleton className="h-4 w-1/2 rounded-xl" />
          <div className="flex gap-2">
            <Skeleton className="h-5 w-20 rounded-full" />
            <Skeleton className="h-5 w-16 rounded-full" />
          </div>
        </div>
      </div>
      <div className="flex-1 p-4 space-y-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <div key={i} className={cn('flex', i % 2 === 0 ? 'justify-start' : 'justify-end')}>
            <Skeleton className={cn('h-16 rounded-xl', i % 2 === 0 ? 'w-2/3' : 'w-1/2')} />
          </div>
        ))}
      </div>
    </div>
  )
}
