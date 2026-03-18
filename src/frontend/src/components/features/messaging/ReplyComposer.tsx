'use client'

import { useEffect, useRef, useState } from 'react'
import { useTranslations } from 'next-intl'
import { Send, StickyNote, Paperclip, Loader2, Check } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { cn } from '@/lib/utils'

const MAX_CHARS = 2000

interface ReplyComposerProps {
  prefillText: string
  onPrefillConsumed: () => void
  isSending: boolean
  canAddNote: boolean
  canAttachToRecord: boolean
  onSendReply: (body: string) => Promise<void>
  onAddNote: (body: string) => Promise<void>
}

export function ReplyComposer({
  prefillText,
  onPrefillConsumed,
  isSending,
  canAddNote,
  canAttachToRecord,
  onSendReply,
  onAddNote,
}: ReplyComposerProps) {
  const t = useTranslations('messaging')
  const [text, setText] = useState('')
  const [sendSuccess, setSendSuccess] = useState(false)
  const textareaRef = useRef<HTMLTextAreaElement>(null)
  const sendSuccessTimeout = useRef<ReturnType<typeof setTimeout> | null>(null)

  // Clean up send success timeout on unmount
  useEffect(() => {
    return () => {
      if (sendSuccessTimeout.current) clearTimeout(sendSuccessTimeout.current)
    }
  }, [])

  // When an AI suggestion is selected, prefill the textarea.
  // We intentionally sync external prop → state here; onPrefillConsumed resets the prop.
  useEffect(() => {
    if (prefillText) {
      // eslint-disable-next-line react-hooks/set-state-in-effect
      setText(prefillText)
      onPrefillConsumed()
      textareaRef.current?.focus()
    }
  }, [prefillText, onPrefillConsumed])

  const charCount = text.length
  const isOverLimit = charCount > MAX_CHARS
  const isEmpty = text.trim().length === 0

  const handleSend = async () => {
    if (isEmpty || isOverLimit || isSending) return
    await onSendReply(text.trim())
    setText('')
    setSendSuccess(true)
    if (sendSuccessTimeout.current) clearTimeout(sendSuccessTimeout.current)
    sendSuccessTimeout.current = setTimeout(() => setSendSuccess(false), 1500)
  }

  const handleAddNote = async () => {
    if (isEmpty || isOverLimit || isSending || !canAddNote) return
    await onAddNote(text.trim())
    setText('')
  }

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && (e.ctrlKey || e.metaKey) && !isEmpty && !isOverLimit) {
      e.preventDefault()
      handleSend()
    }
  }

  return (
    <div className="p-3" data-testid="reply-composer">
      <Textarea
        ref={textareaRef}
        data-testid="reply-textarea"
        value={text}
        onChange={(e) => setText(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder={t('reply_placeholder')}
        className={cn(
          'resize-none min-h-[80px] max-h-[200px] rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-[#303ef5]/20 focus:border-[#303ef5]/50 transition-shadow',
          isOverLimit && 'border-red-400 focus-visible:ring-red-400'
        )}
        disabled={isSending}
        aria-label={t('reply_placeholder')}
        maxLength={MAX_CHARS + 100}
      />

      <div className="flex items-center justify-between mt-2 gap-2">
        {/* Character counter */}
        <span
          data-testid="char-counter"
          className={cn(
            'text-xs tabular-nums',
            isOverLimit ? 'text-red-500 font-semibold' : 'text-muted-foreground'
          )}
          aria-live="polite"
        >
          {charCount}/{MAX_CHARS}
        </span>

        <div className="flex items-center gap-2">
          {/* Attach to medical record — VetOrAdmin only */}
          {canAttachToRecord && (
            <Button
              type="button"
              variant="ghost"
              size="sm"
              data-testid="add-to-record-btn"
              disabled={isSending || isEmpty}
              aria-label={t('attach_to_record')}
              title={t('attach_to_record')}
            >
              <Paperclip className="h-4 w-4" />
              <span className="sr-only">{t('attach_to_record')}</span>
            </Button>
          )}

          {/* Add internal note — VetOrAdmin only */}
          {canAddNote && (
            <Button
              type="button"
              variant="outline"
              size="sm"
              data-testid="add-note-btn"
              disabled={isSending || isEmpty || isOverLimit}
              onClick={handleAddNote}
              aria-label={t('add_note')}
              className="rounded-xl text-[12px] font-semibold border-border/80 hover:bg-[#f4f6f9]"
            >
              <StickyNote className="h-4 w-4 mr-1.5" aria-hidden />
              {t('add_note')}
            </Button>
          )}

          <Button
            type="button"
            size="sm"
            data-testid="send-reply-btn"
            disabled={isSending || isEmpty || isOverLimit}
            onClick={handleSend}
            className="bg-[#303ef5] hover:bg-[#2530c4] text-white font-semibold rounded-xl shadow-sm"
          >
            {isSending ? (
              <Loader2 className="h-4 w-4 mr-1.5 animate-spin" aria-hidden />
            ) : sendSuccess ? (
              <Check className="h-4 w-4 mr-1.5 animate-success-check text-green-200" aria-hidden />
            ) : (
              <Send className="h-4 w-4 mr-1.5" aria-hidden />
            )}
            {isSending ? t('sending') : sendSuccess ? t('send_reply') : t('send_reply')}
          </Button>
        </div>
      </div>

      {isOverLimit && (
        <p className="text-xs text-red-500 mt-1 animate-slide-up-fade" role="alert" data-testid="char-limit-warning">
          {t('char_limit_exceeded')}
        </p>
      )}
    </div>
  )
}
