'use client'

import { useState, useEffect, useRef, useCallback } from 'react'
import { useTranslations } from 'next-intl'
import { Download } from 'lucide-react'
import { cn } from '@/lib/utils'
import type { MessageDto, MessageAttachmentDto, MessageClassificationDto } from '@/lib/api/messaging-types'
import { MessageClassificationBadges } from './MessageClassification'

// ─── Relative time formatter ──────────────────────────────────────────────────

function formatRelativeTime(isoDate: string): string {
  const diffMs = Date.now() - new Date(isoDate).getTime()
  const diffMin = Math.floor(diffMs / 60_000)
  if (diffMin < 1) return 'just now'
  if (diffMin < 60) return `${diffMin} minute${diffMin === 1 ? '' : 's'} ago`
  const diffH = Math.floor(diffMin / 60)
  if (diffH < 24) return `${diffH} hour${diffH === 1 ? '' : 's'} ago`
  const diffD = Math.floor(diffH / 24)
  return `${diffD} day${diffD === 1 ? '' : 's'} ago`
}

function formatFullDate(isoDate: string): string {
  return new Intl.DateTimeFormat('en-AE', {
    timeZone: 'Asia/Dubai',
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(isoDate))
}

// ─── Attachment preview ───────────────────────────────────────────────────────

function LightboxDialog({ attachment, onClose }: { attachment: MessageAttachmentDto; onClose: () => void }) {
  const dialogRef = useRef<HTMLDivElement>(null)
  const closeButtonRef = useRef<HTMLButtonElement>(null)

  useEffect(() => {
    // Auto-focus close button on open
    closeButtonRef.current?.focus()
  }, [])

  const handleKeyDown = useCallback((e: React.KeyboardEvent) => {
    if (e.key === 'Escape') {
      e.preventDefault()
      onClose()
      return
    }
    // Focus trap: cycle focus within dialog
    if (e.key === 'Tab') {
      const dialog = dialogRef.current
      if (!dialog) return
      const focusable = dialog.querySelectorAll<HTMLElement>(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
      )
      if (focusable.length === 0) return
      const first = focusable[0]
      const last = focusable[focusable.length - 1]
      if (e.shiftKey) {
        if (document.activeElement === first) {
          e.preventDefault()
          last.focus()
        }
      } else {
        if (document.activeElement === last) {
          e.preventDefault()
          first.focus()
        }
      }
    }
  }, [onClose])

  return (
    <div
      ref={dialogRef}
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/80"
      role="dialog"
      aria-modal="true"
      aria-label={`Image: ${attachment.fileName}`}
      data-testid="lightbox"
      onClick={() => onClose()}
      onKeyDown={handleKeyDown}
    >
      <button
        ref={closeButtonRef}
        type="button"
        className="absolute top-4 right-4 text-white text-xl font-bold"
        data-testid="lightbox-close"
        onClick={() => onClose()}
        aria-label="Close lightbox"
      >
        ×
      </button>
      {/* eslint-disable-next-line @next/next/no-img-element */}
      <img
        src={attachment.url}
        alt={attachment.fileName}
        className="max-w-[90vw] max-h-[90vh] object-contain rounded"
        onClick={(e) => e.stopPropagation()}
      />
    </div>
  )
}

function AttachmentPreview({ attachment }: { attachment: MessageAttachmentDto }) {
  const [lightboxOpen, setLightboxOpen] = useState(false)
  const isImage = attachment.contentType.startsWith('image/')

  return (
    <>
      <button
        type="button"
        data-testid={`attachment-${attachment.id}`}
        className="mt-2 rounded-xl overflow-hidden border border-border/80 focus:outline-none focus:ring-2 focus:ring-primary/20"
        onClick={() => isImage && setLightboxOpen(true)}
        aria-label={`View attachment: ${attachment.fileName}`}
      >
        {isImage ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={attachment.url}
            alt={attachment.fileName}
            className="max-w-[200px] max-h-[150px] object-cover"
            onError={(e) => {
              // Fallback for missing mock images
              const target = e.currentTarget
              target.style.display = 'none'
              const parent = target.parentElement
              if (parent) {
                parent.innerHTML = `<span class="block px-3 py-2 text-xs text-muted-foreground bg-muted">${attachment.fileName}</span>`
              }
            }}
          />
        ) : (
          <span className="block px-3 py-2 text-xs text-muted-foreground bg-muted">
            {attachment.fileName}
          </span>
        )}
      </button>

      {/* Lightbox */}
      {lightboxOpen && isImage && (
        <LightboxDialog attachment={attachment} onClose={() => setLightboxOpen(false)} />
      )}
    </>
  )
}

// ─── Download all button ──────────────────────────────────────────────────

function DownloadAllButton({ attachments }: { attachments: MessageAttachmentDto[] }) {
  const t = useTranslations('messaging')
  if (attachments.length < 2) return null

  const handleDownloadAll = () => {
    for (const att of attachments) {
      const link = document.createElement('a')
      link.href = att.url
      link.download = att.fileName
      link.target = '_blank'
      link.rel = 'noopener noreferrer'
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
    }
  }

  return (
    <button
      type="button"
      className="flex items-center gap-1 mt-1 text-xs text-muted-foreground hover:text-foreground transition-colors"
      data-testid="download-all-btn"
      onClick={handleDownloadAll}
      aria-label={t('download_all')}
    >
      <Download className="h-3 w-3" aria-hidden />
      {t('download_all')}
    </button>
  )
}

// ─── Message bubble ───────────────────────────────────────────────────────────

interface MessageBubbleProps {
  message: MessageDto
  onClassificationUpdate?: (messageId: string, classification: MessageClassificationDto) => void
}

export function MessageBubble({ message, onClassificationUpdate }: MessageBubbleProps) {
  const t = useTranslations('messaging')
  const [showFullDate, setShowFullDate] = useState(false)
  const [localClassification, setLocalClassification] = useState(message.classification ?? null)

  const isOwner = message.sender === 'Owner'
  const isSystem = message.sender === 'System'
  const isInternalNote = message.isInternalNote

  const handleClassificationUpdate = (updated: MessageClassificationDto) => {
    setLocalClassification(updated)
    onClassificationUpdate?.(message.id, updated)
  }

  // System messages: centered, no bubble
  if (isSystem) {
    return (
      <div
        className="flex justify-center my-2 animate-slide-up-fade"
        data-testid={`message-${message.id}`}
        data-sender="system"
      >
        <span className="text-[11px] text-muted-foreground italic bg-muted px-3 py-1.5 rounded-full font-medium">
          {message.body}
        </span>
      </div>
    )
  }

  // Internal note: distinctive yellow background
  if (isInternalNote) {
    return (
      <div
        className="flex justify-end my-2 animate-slide-up-fade"
        data-testid={`message-${message.id}`}
        data-sender="internal-note"
      >
        <div className="max-w-[80%]">
          <div className="flex items-center gap-1 mb-1 justify-end">
            <span className="text-xs font-medium text-yellow-700 bg-yellow-100 px-2 py-0.5 rounded-full border border-yellow-200">
              {t('internal_note')}
            </span>
            {message.senderName && (
              <span className="text-xs text-muted-foreground">{message.senderName}</span>
            )}
          </div>
          <div className="bg-yellow-50 border border-yellow-200 rounded-xl px-4 py-3 text-[13px] text-yellow-900">
            {message.body}
          </div>
          <div className="flex justify-end mt-1">
            <button
              type="button"
              className="text-xs text-muted-foreground hover:text-foreground transition-colors"
              onClick={() => setShowFullDate((p) => !p)}
              data-testid={`message-timestamp-${message.id}`}
              aria-label={showFullDate ? 'Show relative time' : 'Show full date and time'}
            >
              {showFullDate ? formatFullDate(message.sentAt) : formatRelativeTime(message.sentAt)}
            </button>
          </div>
        </div>
      </div>
    )
  }

  // Owner message: left-aligned, light background
  if (isOwner) {
    return (
      <div
        className="flex justify-start my-2 animate-slide-up-fade"
        data-testid={`message-${message.id}`}
        data-sender="owner"
      >
        <div className="max-w-[80%]">
          {message.senderName && (
            <p className="text-xs text-muted-foreground mb-1 ml-1">{message.senderName}</p>
          )}
          <div className="bg-muted rounded-xl px-4 py-3 text-[13px] text-foreground">
            {message.body}
            {message.attachments.length > 0 && (
              <div className="flex flex-wrap gap-2 mt-2">
                {message.attachments.map((att) => (
                  <AttachmentPreview key={att.id} attachment={att} />
                ))}
              </div>
            )}
          </div>
          {message.attachments.length > 0 && (
            <DownloadAllButton attachments={message.attachments} />
          )}
          <button
            type="button"
            className="text-xs text-muted-foreground hover:text-foreground transition-colors mt-1 ml-1"
            onClick={() => setShowFullDate((p) => !p)}
            data-testid={`message-timestamp-${message.id}`}
            aria-label={showFullDate ? 'Show relative time' : 'Show full date and time'}
          >
            {showFullDate ? formatFullDate(message.sentAt) : formatRelativeTime(message.sentAt)}
          </button>
          {localClassification && (
            <MessageClassificationBadges
              messageId={message.id}
              conversationId={message.conversationId}
              classification={localClassification}
              align="left"
              onClassificationUpdate={handleClassificationUpdate}
            />
          )}
        </div>
      </div>
    )
  }

  // Staff / Vet message: right-aligned, colored background
  return (
    <div
      className="flex justify-end my-2 animate-slide-up-fade"
      data-testid={`message-${message.id}`}
      data-sender={message.sender.toLowerCase()}
    >
      <div className="max-w-[80%]">
        {message.senderName && (
          <p className="text-xs text-muted-foreground mb-1 mr-1 text-right">{message.senderName}</p>
        )}
        <div
          className={cn(
            'rounded-xl px-4 py-3 text-[13px] text-white',
            message.sender === 'Vet' ? 'bg-primary' : 'bg-primary/80'
          )}
        >
          {message.body}
          {message.attachments.length > 0 && (
            <div className="flex flex-wrap gap-2 mt-2">
              {message.attachments.map((att) => (
                <AttachmentPreview key={att.id} attachment={att} />
              ))}
            </div>
          )}
        </div>
        {message.attachments.length > 0 && (
          <div className="flex justify-end">
            <DownloadAllButton attachments={message.attachments} />
          </div>
        )}
        <div className="flex justify-end mt-1">
          <button
            type="button"
            className="text-xs text-muted-foreground hover:text-foreground transition-colors"
            onClick={() => setShowFullDate((p) => !p)}
            data-testid={`message-timestamp-${message.id}`}
            aria-label={showFullDate ? 'Show relative time' : 'Show full date and time'}
          >
            {showFullDate ? formatFullDate(message.sentAt) : formatRelativeTime(message.sentAt)}
          </button>
        </div>
        {localClassification && (
          <MessageClassificationBadges
            messageId={message.id}
            conversationId={message.conversationId}
            classification={localClassification}
            align="right"
            onClassificationUpdate={handleClassificationUpdate}
          />
        )}
      </div>
    </div>
  )
}
