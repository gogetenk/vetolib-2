'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { Copy, Check, Share2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogTrigger,
} from '@/components/ui/dialog'
import { QrCode } from './QrCode'
import { createShareLink } from '@/lib/api/record-sharing'
import type { ShareLinkDto } from '@/lib/api/record-sharing'

interface ShareRecordsModalProps {
  animalId: string
}

export function ShareRecordsModal({ animalId }: ShareRecordsModalProps) {
  const t = useTranslations('portal.sharing')
  const [open, setOpen] = useState(false)
  const [loading, setLoading] = useState(false)
  const [shareLink, setShareLink] = useState<ShareLinkDto | null>(null)
  const [copied, setCopied] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleOpen(isOpen: boolean) {
    setOpen(isOpen)
    if (isOpen && !shareLink) {
      setLoading(true)
      setError(null)
      try {
        const link = await createShareLink(animalId)
        setShareLink(link)
      } catch {
        setError(t('create_error'))
      } finally {
        setLoading(false)
      }
    }
  }

  async function handleCopy() {
    if (!shareLink) return
    try {
      await navigator.clipboard.writeText(shareLink.shareUrl)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    } catch {
      // Fallback for environments without clipboard API
      const textarea = document.createElement('textarea')
      textarea.value = shareLink.shareUrl
      document.body.appendChild(textarea)
      textarea.select()
      document.execCommand('copy')
      document.body.removeChild(textarea)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    }
  }

  function formatExpiry(expiresAt: string): string {
    const expiry = new Date(expiresAt)
    const now = new Date()
    const hoursLeft = Math.max(0, Math.round((expiry.getTime() - now.getTime()) / (1000 * 60 * 60)))
    return t('expires_in', { hours: hoursLeft })
  }

  return (
    <Dialog open={open} onOpenChange={handleOpen}>
      <DialogTrigger
        render={
          <Button
            variant="outline"
            size="sm"
            data-testid="share-records-button"
          />
        }
      >
        <Share2 className="h-4 w-4 me-1" />
        {t('share_records')}
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t('modal_title')}</DialogTitle>
          <DialogDescription>{t('modal_description')}</DialogDescription>
        </DialogHeader>

        {loading && (
          <div className="flex items-center justify-center py-8" data-testid="share-loading">
            <div className="h-8 w-8 animate-spin rounded-full border-4 border-muted border-t-primary" />
          </div>
        )}

        {error && (
          <div className="text-center py-4" data-testid="share-error">
            <p className="text-sm text-destructive">{error}</p>
          </div>
        )}

        {shareLink && !loading && (
          <div className="space-y-4">
            {/* QR Code */}
            <div className="flex justify-center">
              <div className="rounded-lg border border-border p-3 bg-white">
                <QrCode
                  value={shareLink.shareUrl}
                  size={180}
                  data-testid="share-qr-code"
                />
              </div>
            </div>

            {/* Copyable link */}
            <div className="flex items-center gap-2">
              <input
                type="text"
                readOnly
                value={shareLink.shareUrl}
                className="flex-1 rounded-md border border-input bg-muted/50 px-3 py-2 text-xs font-mono truncate"
                data-testid="share-link-input"
              />
              <Button
                variant="outline"
                size="sm"
                onClick={handleCopy}
                data-testid="share-link-copy"
              >
                {copied ? (
                  <Check className="h-4 w-4 text-success" />
                ) : (
                  <Copy className="h-4 w-4" />
                )}
              </Button>
            </div>

            {/* Expiry info */}
            <p className="text-xs text-muted-foreground text-center" data-testid="share-expiry">
              {formatExpiry(shareLink.expiresAt)}
            </p>
          </div>
        )}
      </DialogContent>
    </Dialog>
  )
}
