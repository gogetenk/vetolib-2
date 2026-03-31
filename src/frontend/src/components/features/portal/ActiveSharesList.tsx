'use client'

import { useEffect, useState } from 'react'
import { useTranslations, useLocale } from 'next-intl'
import { Link2, Trash2, Eye } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { listShareLinks, revokeShareLink } from '@/lib/api/record-sharing'
import type { ShareLinkDto } from '@/lib/api/record-sharing'

export function ActiveSharesList() {
  const t = useTranslations('portal.sharing')
  const locale = useLocale()
  const [shares, setShares] = useState<ShareLinkDto[]>([])
  const [loading, setLoading] = useState(true)
  const [revokingId, setRevokingId] = useState<string | null>(null)

  useEffect(() => {
    listShareLinks()
      .then(setShares)
      .catch(() => {
        // Silently fail — shares section is optional
      })
      .finally(() => setLoading(false))
  }, [])

  async function handleRevoke(id: string) {
    setRevokingId(id)
    try {
      await revokeShareLink(id)
      setShares(prev => prev.filter(s => s.id !== id))
    } catch {
      // Failed to revoke
    } finally {
      setRevokingId(null)
    }
  }

  if (loading) {
    return (
      <div className="space-y-2" data-testid="shares-loading">
        {[1, 2].map(i => (
          <div key={i} className="h-16 rounded-lg bg-muted animate-pulse" />
        ))}
      </div>
    )
  }

  if (shares.length === 0) {
    return null
  }

  return (
    <div className="space-y-3" data-testid="active-shares-list">
      <h3 className="text-sm font-semibold text-foreground flex items-center gap-2">
        <Link2 className="h-4 w-4" />
        {t('active_links')}
      </h3>
      <ul className="space-y-2">
        {shares.map(share => {
          const isExpired = new Date(share.expiresAt) < new Date()
          return (
            <li
              key={share.id}
              className="flex items-center gap-3 rounded-lg border border-border/80 px-3 py-2 bg-white"
              data-testid={`share-item-${share.id}`}
            >
              <div className="flex-1 min-w-0">
                <p className="text-xs font-mono text-muted-foreground truncate">
                  {share.shareUrl}
                </p>
                <div className="flex items-center gap-3 mt-1 text-xs text-muted-foreground">
                  <span data-testid={`share-created-${share.id}`}>
                    {t('created', {
                      date: new Date(share.createdAt).toLocaleDateString(locale, {
                        day: 'numeric',
                        month: 'short',
                        timeZone: 'Asia/Dubai',
                      }),
                    })}
                  </span>
                  <span
                    className={isExpired ? 'text-destructive' : ''}
                    data-testid={`share-expiry-${share.id}`}
                  >
                    {isExpired
                      ? t('expired')
                      : t('expires', {
                          date: new Date(share.expiresAt).toLocaleDateString(locale, {
                            day: 'numeric',
                            month: 'short',
                            hour: '2-digit',
                            minute: '2-digit',
                            timeZone: 'Asia/Dubai',
                          }),
                        })}
                  </span>
                  <span className="flex items-center gap-1" data-testid={`share-views-${share.id}`}>
                    <Eye className="h-3 w-3" />
                    {t('views', { count: share.accessCount })}
                  </span>
                </div>
              </div>
              <Button
                variant="ghost"
                size="icon-sm"
                onClick={() => handleRevoke(share.id)}
                disabled={revokingId === share.id}
                data-testid={`share-revoke-${share.id}`}
                aria-label={t('revoke')}
              >
                <Trash2 className="h-4 w-4 text-destructive" />
              </Button>
            </li>
          )
        })}
      </ul>
    </div>
  )
}
