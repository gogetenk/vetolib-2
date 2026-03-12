'use client'

import { useEffect, useState } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter, useParams } from 'next/navigation'
import Link from 'next/link'
import { MessageCircle, Plus, ChevronRight } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { listPortalConversations } from '@/lib/api/portal'
import { ApiError } from '@/lib/api/client'
import type { PortalConversationDto, ConversationStatus } from '@/lib/api/messaging-types'

const STATUS_COLORS: Record<ConversationStatus, string> = {
  Open: 'bg-emerald-100 text-emerald-800',
  InProgress: 'bg-blue-100 text-blue-800',
  Resolved: 'bg-gray-100 text-gray-600',
  Closed: 'bg-gray-100 text-gray-500',
}

export function PortalLanding() {
  const t = useTranslations('portal.landing')
  const router = useRouter()
  const params = useParams<{ locale: string; clinicSlug: string }>()
  const [conversations, setConversations] = useState<PortalConversationDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [expired, setExpired] = useState(false)

  useEffect(() => {
    listPortalConversations()
      .then(setConversations)
      .catch((err) => {
        if (err instanceof ApiError && err.status === 401) {
          setExpired(true)
        }
      })
      .finally(() => setIsLoading(false))
  }, [])

  function handleNewMessage() {
    const consentGiven = sessionStorage.getItem('portal_consent_given')
    if (!consentGiven) {
      router.push(`/${params.locale}/portal/${params.clinicSlug}/consent`)
    } else {
      router.push(`/${params.locale}/portal/${params.clinicSlug}/new`)
    }
  }

  if (expired) {
    return (
      <div
        className="text-center py-12 space-y-3"
        data-testid="portal-expired"
      >
        <MessageCircle className="w-12 h-12 text-gray-400 mx-auto" />
        <p className="text-gray-700 font-medium" data-testid="expired-message">
          {t('link_expired')}
        </p>
      </div>
    )
  }

  return (
    <div className="space-y-5" data-testid="portal-landing">
      {/* Header row */}
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-gray-900" data-testid="portal-landing-title">
          {t('your_conversations')}
        </h1>
        <Button
          onClick={handleNewMessage}
          data-testid="new-message-btn"
          className="bg-emerald-600 hover:bg-emerald-700 text-white"
          size="sm"
        >
          <Plus className="h-4 w-4 me-1" />
          {t('new_message')}
        </Button>
      </div>

      {/* Conversation list */}
      {isLoading ? (
        <div className="space-y-3" data-testid="portal-loading">
          {[1, 2].map((i) => (
            <div key={i} className="h-20 rounded-lg bg-gray-200 animate-pulse" />
          ))}
          <p className="text-sm text-gray-500 text-center">{t('loading')}</p>
        </div>
      ) : conversations.length === 0 ? (
        <div className="text-center py-12 text-gray-500" data-testid="portal-empty">
          <MessageCircle className="w-10 h-10 mx-auto mb-2 text-gray-300" />
          <p className="text-sm">{t('no_conversations')}</p>
        </div>
      ) : (
        <ul className="space-y-2" data-testid="portal-conversations-list">
          {conversations.map((conv) => (
            <li key={conv.id}>
              <Link
                href={`/${params.locale}/portal/${params.clinicSlug}/conversations/${conv.id}`}
                data-testid={`conversation-item-${conv.id}`}
                className="flex items-center gap-3 bg-white rounded-lg border border-gray-200 px-4 py-3 hover:border-emerald-300 hover:shadow-sm transition-all"
              >
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 mb-1">
                    <span
                      className={`text-xs px-2 py-0.5 rounded-full font-medium ${STATUS_COLORS[conv.status]}`}
                      data-testid={`conv-status-${conv.id}`}
                    >
                      {t(`status.${conv.status}`)}
                    </span>
                    {conv.unreadByOwnerCount > 0 && (
                      <span
                        className="text-xs bg-emerald-600 text-white px-1.5 py-0.5 rounded-full font-semibold"
                        data-testid={`conv-unread-${conv.id}`}
                      >
                        {t('unread', { count: conv.unreadByOwnerCount })}
                      </span>
                    )}
                  </div>
                  <p
                    className="text-sm font-medium text-gray-900 truncate"
                    data-testid={`conv-subject-${conv.id}`}
                  >
                    {conv.subject}
                  </p>
                  <p className="text-xs text-gray-500 mt-0.5">
                    {t('last_message', {
                      date: new Date(conv.lastMessageAt).toLocaleDateString('en-AE', {
                        day: 'numeric',
                        month: 'short',
                        year: 'numeric',
                        timeZone: 'Asia/Dubai',
                      }),
                    })}
                  </p>
                </div>
                <ChevronRight className="h-4 w-4 text-gray-400 flex-shrink-0" />
              </Link>
            </li>
          ))}
        </ul>
      )}

      {/* Export link */}
      <div className="text-center pt-4 border-t border-gray-100">
        <Link
          href={`/${params.locale}/portal/${params.clinicSlug}/export`}
          className="text-sm text-emerald-600 hover:text-emerald-800 hover:underline"
          data-testid="export-link"
        >
          {t('download_all')}
        </Link>
      </div>
    </div>
  )
}
