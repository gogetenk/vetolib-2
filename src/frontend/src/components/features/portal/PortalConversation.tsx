'use client'

import { useEffect, useState, useRef } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter, useParams } from 'next/navigation'
import { ArrowLeft, Send } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { getPortalConversation, sendPortalMessage } from '@/lib/api/portal'
import { ApiError } from '@/lib/api/client'
import type { PortalConversationDto, PortalMessageDto } from '@/lib/api/messaging-types'

interface PortalConversationProps {
  conversationId: string
}

export function PortalConversation({ conversationId }: PortalConversationProps) {
  const t = useTranslations('portal.conversation')
  const router = useRouter()
  const params = useParams<{ locale: string; clinicSlug: string }>()

  const [conversation, setConversation] = useState<PortalConversationDto | null>(null)
  const [messages, setMessages] = useState<PortalMessageDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [expired, setExpired] = useState(false)
  const [reply, setReply] = useState('')
  const [isSending, setIsSending] = useState(false)
  const [sendError, setSendError] = useState<string | null>(null)
  const bottomRef = useRef<HTMLDivElement>(null)
  const tLanding = useTranslations('portal.landing')

  useEffect(() => {
    getPortalConversation(conversationId)
      .then((conv) => {
        setConversation(conv)
        setMessages(conv.messages ?? [])
      })
      .catch((err) => {
        if (err instanceof ApiError && err.status === 401) {
          setExpired(true)
        }
        // For 404 or other errors, conversation stays null → shows not_found
      })
      .finally(() => setIsLoading(false))
  }, [conversationId])

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  const isClosed = conversation?.status === 'Closed'

  async function handleSendReply() {
    if (!reply.trim()) return
    setIsSending(true)
    setSendError(null)
    try {
      const newMsg = await sendPortalMessage(conversationId, { body: reply.trim() })
      setMessages((prev) => [...prev, newMsg])
      setReply('')
    } catch (err) {
      if (err instanceof ApiError && err.status === 429) {
        setSendError(t('errors.too_many_messages'))
      } else {
        setSendError(t('errors.send_failed'))
      }
    } finally {
      setIsSending(false)
    }
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12" data-testid="conversation-loading">
        <div className="h-8 w-8 rounded-full border-2 border-primary border-t-transparent animate-spin" />
      </div>
    )
  }

  if (expired) {
    return (
      <div className="text-center py-12 space-y-3" data-testid="conversation-expired">
        <p className="text-foreground">{tLanding('link_expired')}</p>
      </div>
    )
  }

  if (!conversation) {
    return (
      <div className="text-center py-12 text-muted-foreground" data-testid="conversation-not-found">
        <p>{t('not_found')}</p>
        <Button
          variant="ghost"
          onClick={() => router.push(`/${params.locale}/portal/${params.clinicSlug}`)}
          className="mt-3"
          data-testid="portal-conversation-back-btn"
        >
          {t('back')}
        </Button>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-4" data-testid="portal-conversation">
      {/* Back + title */}
      <div className="flex items-center gap-2">
        <button
          type="button"
          onClick={() => router.push(`/${params.locale}/portal/${params.clinicSlug}`)}
          data-testid="back-to-conversations-link"
          className="flex items-center gap-1 text-[13px] text-primary hover:text-primary/90"
        >
          <ArrowLeft className="h-4 w-4" />
          {t('back')}
        </button>
      </div>

      <div>
        <h1 className="text-[18px] font-bold text-foreground" data-testid="conversation-subject">
          {conversation.subject}
        </h1>
        <p className="text-xs text-muted-foreground mt-0.5">
          {new Date(conversation.createdAt).toLocaleDateString('en-AE', {
            day: 'numeric',
            month: 'long',
            year: 'numeric',
            timeZone: 'Asia/Dubai',
          })}
        </p>
      </div>

      {/* Message thread */}
      <div className="space-y-3" data-testid="message-thread">
        {messages.map((msg) => {
          const isOwner = msg.sender === 'Owner'
          return (
            <div
              key={msg.id}
              className={`flex ${isOwner ? 'justify-end' : 'justify-start'}`}
              data-testid={`message-${msg.id}`}
            >
              <div
                className={`max-w-[85%] rounded-2xl px-4 py-2.5 text-[13px] ${
                  isOwner
                    ? 'bg-primary text-white rounded-br-sm'
                    : 'bg-white border border-border/80 text-foreground rounded-bl-sm shadow-sm'
                }`}
              >
                {!isOwner && (
                  <p className="text-xs font-semibold mb-1 text-primary">
                    {msg.senderName ?? t('clinic')}
                  </p>
                )}
                <p className="whitespace-pre-wrap">{msg.body}</p>
                <p className={`text-xs mt-1 ${isOwner ? 'text-blue-100' : 'text-muted-foreground'}`}>
                  {new Date(msg.sentAt).toLocaleTimeString('en-AE', {
                    hour: '2-digit',
                    minute: '2-digit',
                    timeZone: 'Asia/Dubai',
                  })}
                </p>
              </div>
            </div>
          )
        })}
        <div ref={bottomRef} />
      </div>

      {/* Closed notice or reply box */}
      {isClosed ? (
        <div
          className="rounded-xl bg-muted border border-border/80 px-4 py-3 text-[13px] text-muted-foreground text-center"
          data-testid="conversation-closed-notice"
        >
          {t('closed_notice')}
        </div>
      ) : (
        <div className="space-y-2" data-testid="reply-area">
          {sendError && (
            <p className="text-sm text-red-600" data-testid="reply-error">
              {sendError}
            </p>
          )}
          <div className="flex gap-2 items-end">
            <textarea
              value={reply}
              onChange={(e) => setReply(e.target.value)}
              placeholder={t('reply_placeholder')}
              rows={3}
              data-testid="reply-input"
              className="flex-1 block rounded-xl border border-border/80 px-3 py-2 text-[13px] shadow-sm focus:outline-none focus:ring-1 focus:ring-primary focus:border-primary resize-none"
              onKeyDown={(e) => {
                if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
                  handleSendReply()
                }
              }}
            />
            <Button
              onClick={handleSendReply}
              disabled={isSending || !reply.trim()}
              data-testid="send-reply-btn"
              className="bg-primary hover:bg-primary/90 text-white font-semibold rounded-xl shadow-sm h-10 px-3"
            >
              {isSending ? (
                <div className="h-4 w-4 rounded-full border-2 border-white border-t-transparent animate-spin" />
              ) : (
                <Send className="h-4 w-4" />
              )}
              <span className="sr-only">{t('send_reply')}</span>
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
